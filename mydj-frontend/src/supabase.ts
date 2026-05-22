import { createClient, SupabaseClient } from '@supabase/supabase-js'
import type { Track } from './api'

const supabaseUrl = import.meta.env.VITE_SUPABASE_URL as string
const supabaseAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY as string

export const supabase: SupabaseClient = createClient(supabaseUrl, supabaseAnonKey)

export const upsertSongs = async (tracks: Track[]) => {
  const rows = tracks.map((track) => ({
    file_path: track.file_path,
    title: track.title,
    duration: track.duration,
    artist: track.artist || 'Unknown',
    liked: track.liked ?? false
  }))

  await supabase.from('songs').upsert(rows, { onConflict: 'file_path' })
}

export const insertPlayHistory = async (songId: number | string) => {
  await supabase.from('play_history').insert([{ song_id: songId, played_at: new Date().toISOString() }])
}

export const toggleFavorite = async (songId: number | string, liked: boolean) => {
  await supabase.from('songs').update({ liked }).eq('file_path', songId)
}

export const subscribePlayHistory = (onUpdate: (payload: any) => void) => {
  const channel = supabase
    .channel('play_history_channel')
    .on(
      'postgres_changes',
      { event: '*', schema: 'public', table: 'play_history' },
      (payload) => {
        onUpdate(payload)
      }
    )
    .subscribe()

  return channel
}
