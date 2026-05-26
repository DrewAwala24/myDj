import { useCallback, useEffect, useMemo, useState } from 'react'
import { getSongs, getNowPlaying, playSong, pauseSong, stopSong, shuffleSongs, Track } from './api'
import { supabase, upsertSongs, insertPlayHistory, subscribePlayHistory, toggleFavorite } from './supabase'
import Vinyl from './components/Vinyl'
import Waveform from './components/Waveform'
import Controls from './components/Controls'
import TrackList from './components/TrackList'
import EQKnobs from './components/EQKnobs'

const displayStatus = (isPlaying: boolean, progress: number) => {
  if (isPlaying) return 'PLAYING'
  if (progress > 0) return 'PAUSED'
  return 'READY'
}

const randomWaveform = () => Array.from({ length: 32 }, () => 12 + Math.round(Math.random() * 72))

function App() {
  const [tracks, setTracks] = useState<Track[]>([])
  const [currentTrackIndex, setCurrentTrackIndex] = useState(0)
  const [isPlaying, setIsPlaying] = useState(false)
  const [progress, setProgress] = useState(0)
  const [volume, setVolume] = useState(72)
  const [repeat, setRepeat] = useState(false)
  const [shuffle, setShuffle] = useState(false)
  const [history, setHistory] = useState<Array<{ title: string; played_at: string }>>([])
  const [favorites, setFavorites] = useState<string[]>([])
  const [search, setSearch] = useState('')
  const [activeTab, setActiveTab] = useState<'library' | 'history'>('library')
  const [waveform, setWaveform] = useState<number[]>(randomWaveform())

  const currentTrack = tracks[currentTrackIndex]

  const filteredTracks = useMemo(
    () =>
      tracks
        .map((track, index) => ({ track, index }))
        .filter(({ track }) => {
          const query = search.toLowerCase().trim()
          if (!query) return true
          return track.title.toLowerCase().includes(query) || (track.artist ?? '').toLowerCase().includes(query)
        }),
    [search, tracks]
  )

  const loadSongs = useCallback(async () => {
    try {
      const songs = await getSongs()
      setTracks(songs)
      setFavorites(songs.filter((song) => song.liked).map((song) => String(song.file_path)))
      await upsertSongs(songs)
    } catch (error) {
      console.error('Unable to load tracks', error)
    }
  }, [])

  const refreshNowPlaying = useCallback(async () => {
    try {
      const nowPlaying = await getNowPlaying()
      setIsPlaying(nowPlaying.isPlaying)
      setCurrentTrackIndex(nowPlaying.trackIndex)
      setProgress(nowPlaying.progress)
    } catch (error) {
      console.warn('Now playing sync failed', error)
    }
  }, [])

  useEffect(() => {
    loadSongs()
    refreshNowPlaying()
    const interval = window.setInterval(refreshNowPlaying, 2000)
    const channel = subscribePlayHistory((payload) => {
      const record = payload?.new
      if (record) {
        setHistory((prev) => [
          { title: record.title ?? String(record.song_id), played_at: new Date(record.played_at).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) },
          ...prev
        ].slice(0, 20))
      }
    })

    return () => {
      window.clearInterval(interval)
      if (channel && supabase) {
        supabase.removeChannel(channel)
      }
    }
  }, [loadSongs, refreshNowPlaying])

  useEffect(() => {
    setWaveform(randomWaveform())
    setProgress(0)
  }, [currentTrackIndex])

  const handlePlay = async (index?: number) => {
    const songIndex = index !== undefined ? index : currentTrackIndex
    try {
      await playSong(songIndex)
      setCurrentTrackIndex(songIndex)
      setIsPlaying(true)
      setProgress(0)
      const song = tracks[songIndex]
      if (song) {
        await insertPlayHistory(song.file_path)
      }
    } catch (error) {
      console.error('Play action failed', error)
    }
  }

  const handlePause = async () => {
    try {
      await pauseSong()
      setIsPlaying((value) => !value)
    } catch (error) {
      console.error('Pause action failed', error)
    }
  }

  const handleStop = async () => {
    try {
      await stopSong()
      setIsPlaying(false)
      setProgress(0)
    } catch (error) {
      console.error('Stop action failed', error)
    }
  }

  const handleShuffle = async () => {
    try {
      await shuffleSongs()
      setShuffle((value) => !value)
    } catch (error) {
      console.error('Shuffle action failed', error)
    }
  }

  const handleFavorite = async (track: Track) => {
    const trackId = String(track.file_path)
    const liked = favorites.includes(trackId) ? false : true
    setFavorites((prev) => (liked ? [...prev, trackId] : prev.filter((id) => id !== trackId)))
    try {
      await toggleFavorite(trackId, liked)
    } catch (error) {
      console.error('Favorite toggle failed', error)
    }
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">MyDJ</div>
        <div className="status-pill">
          <span className="status-dot" />
          {displayStatus(isPlaying, progress)}
        </div>
      </header>

      <main className="grid-layout">
        <section className="panel">
          <div className="now-playing-title">Now Playing</div>
          <p className="now-playing-subtitle">Full dark audio experience with gold accents and live state sync.</p>

          <div className="vinyl-panel">
            <div className="vinyl-frame">
              <Vinyl isPlaying={isPlaying} title={currentTrack?.title ?? 'No track selected'} />
            </div>
            <Waveform bars={waveform} />
          </div>

          <div className="progress-group">
            <div className="progress-labels">
              <span>{currentTrack?.title ?? 'Idle'}</span>
              <span>{progress}%</span>
            </div>
            <div className="progress-bar">
              <div className="progress-thumb" style={{ width: `${progress}%` }} />
            </div>
          </div>

          <div className="controls-grid">
            <Controls
              isPlaying={isPlaying}
              shuffle={shuffle}
              repeat={repeat}
              onPlay={() => handlePlay()}
              onPause={handlePause}
              onStop={handleStop}
              onShuffle={handleShuffle}
              onRepeat={() => setRepeat((value) => !value)}
              onPrev={() => handlePlay(Math.max(0, currentTrackIndex - 1))}
              onNext={() => handlePlay(Math.min(tracks.length - 1, currentTrackIndex + 1))}
              volume={volume}
              onVolumeChange={(value) => setVolume(value)}
            />

            <div className="track-detail">
              <div>
                <strong>{currentTrack?.title ?? 'Select a track'}</strong>
                <span>{currentTrack?.artist ?? 'Artist not available'}</span>
              </div>
              <div>
                <span>{currentTrack?.duration ?? '--:--'}</span>
              </div>
            </div>

            <div className="slider-group">
              <label>
                Volume
                <span>{volume}%</span>
              </label>
              <input
                type="range"
                min="0"
                max="100"
                value={volume}
                onChange={(event) => setVolume(Number(event.target.value))}
              />
            </div>

            <div className="eq-grid">
              <EQKnobs values={{ LOW: 32, MID: 54, HIGH: 68, GAIN: 42 }} />
            </div>
          </div>
        </section>

        <aside className="sidebar panel">
          <div className="tab-nav">
            <button className={`tab-button ${activeTab === 'library' ? 'active' : ''}`} onClick={() => setActiveTab('library')}>
              Library
            </button>
            <button className={`tab-button ${activeTab === 'history' ? 'active' : ''}`} onClick={() => setActiveTab('history')}>
              History
            </button>
          </div>

          <div className="search-box">
            <span>🔍</span>
            <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search tracks" />
          </div>

          <TrackList
            activeTab={activeTab}
            tracks={filteredTracks}
            history={history}
            currentTrackIndex={currentTrackIndex}
            favorites={favorites}
            onSelect={(index) => handlePlay(index)}
            onToggleFavorite={handleFavorite}
          />
        </aside>
      </main>
    </div>
  )
}

export default App
