import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json'
  }
})

export interface Track {
  id: number | string
  title: string
  duration: string
  artist?: string
  file_path: string
  liked?: boolean
}

export interface NowPlayingResponse {
  trackIndex: number
  isPlaying: boolean
  progress: number
}

export const getSongs = async (): Promise<Track[]> => {
  const response = await api.get<Track[]>('/api/songs')
  return response.data
}

export const playSong = async (songIndex: number) => {
  await api.post('/api/play', { songIndex })
}

export const pauseSong = async () => {
  await api.post('/api/pause')
}

export const stopSong = async () => {
  await api.post('/api/stop')
}

export const shuffleSongs = async () => {
  await api.post('/api/shuffle')
}

export const getNowPlaying = async (): Promise<NowPlayingResponse> => {
  const response = await api.get<NowPlayingResponse>('/api/nowplaying')
  return response.data
}
