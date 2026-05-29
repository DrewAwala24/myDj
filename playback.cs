using System;
using NAudio.Wave;

namespace MyPersonalDjGui
{
    public class playback
    {
        private IWavePlayer waveOutDevice;
        private AudioFileReader audioStream;
        private string currentFilePath = "";

        public void start(string filePath)
        {
            // If the track is already loaded and suspended, hitting play resumes it
            if (currentFilePath == filePath && waveOutDevice != null && waveOutDevice.PlaybackState == PlaybackState.Paused)
            {
                waveOutDevice.Play();
                return;
            }

            // Otherwise, flush old devices before a fresh memory allocation
            Stop();

            try
            {
                currentFilePath = filePath;
                waveOutDevice = new WaveOutEvent();
                audioStream = new AudioFileReader(filePath);
                waveOutDevice.Init(audioStream);
                waveOutDevice.Play();
            }
            catch (Exception ex)
            {
                currentFilePath = "";
                throw new Exception("Initialization failed: " + ex.Message);
            }
        }

        public void pause()
        {
            if (waveOutDevice != null)
            {
                if (waveOutDevice.PlaybackState == PlaybackState.Playing)
                {
                    waveOutDevice.Pause();
                }
                else if (waveOutDevice.PlaybackState == PlaybackState.Paused)
                {
                    waveOutDevice.Play();
                }
            }
        }

        public void Stop()
        {
            try
            {
                waveOutDevice?.Stop();
                waveOutDevice?.Dispose();
                waveOutDevice = null;

                audioStream?.Dispose();
                audioStream = null;

                currentFilePath = "";
            }
            catch { }
        }

        // ── TIME & SEEKING ENGINES ──

        public double GetCurrentTimeInSeconds()
        {
            return audioStream?.CurrentTime.TotalSeconds ?? 0;
        }

        public double GetTotalTimeInSeconds()
        {
            return audioStream?.TotalTime.TotalSeconds ?? 0;
        }

        public void SetPositionInSeconds(double seconds)
        {
            if (audioStream != null)
            {
                // Protect bounds to prevent NAudio from crashing at terminal end file lines
                double target = Math.Clamp(seconds, 0, GetTotalTimeInSeconds() - 0.1);
                audioStream.CurrentTime = TimeSpan.FromSeconds(target);
            }
        }

        public void SetVolume(float volume)
        {
            if (audioStream != null)
            {
                // NAudio takes values from 0.0f to 1.0f
                audioStream.Volume = Math.Clamp(volume, 0.0f, 1.0f);
            }
        }

        public int GetShuffleIndex(int totalSongs)
        {
            if (totalSongs <= 1) return 0;
            Random rand = new Random();
            return rand.Next(0, totalSongs);
        }
    }
}