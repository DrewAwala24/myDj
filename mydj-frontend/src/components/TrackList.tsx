import type { Track } from '../api'

interface TrackListProps {
  activeTab: 'library' | 'history'
  tracks: Array<{ track: Track; index: number }>
  history: Array<{ title: string; played_at: string }>
  currentTrackIndex: number
  favorites: string[]
  onSelect: (index: number) => void
  onToggleFavorite: (track: Track) => void
}

const TrackList = ({ activeTab, tracks, history, currentTrackIndex, favorites, onSelect, onToggleFavorite }: TrackListProps) => {
  if (activeTab === 'history') {
    return (
      <div className="history-list">
        {history.length === 0 ? (
          <div className="history-item">
            <div className="history-meta">
              <strong>No history yet</strong>
              <span>Play songs to populate this feed.</span>
            </div>
          </div>
        ) : (
          history.map((entry, index) => (
            <div key={`${entry.title}-${index}`} className="history-item">
              <div className="history-meta">
                <strong>{entry.title}</strong>
                <span>Played at {entry.played_at}</span>
              </div>
              <div className="history-time">LIVE</div>
            </div>
          ))
        )}
      </div>
    )
  }

  return (
    <div className="track-list">
      {tracks.map(({ track, index }) => {
        const active = index === currentTrackIndex
        const favorite = favorites.includes(String(track.file_path))

        return (
          <div key={track.file_path} className={`track-item ${active ? 'active' : ''}`}>
            <div className="track-number">#{index + 1}</div>
            <div className="track-meta" onClick={() => onSelect(index)} style={{ cursor: 'pointer' }}>
              <span className="track-title">{track.title}</span>
              <span className="track-duration">{track.duration}</span>
            </div>
            <button className={`favorite-button ${favorite ? 'active' : ''}`} onClick={() => onToggleFavorite(track)}>
              {favorite ? '♥' : '♡'}
            </button>
          </div>
        )
      })}
    </div>
  )
}

export default TrackList
