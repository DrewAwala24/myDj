using System;
using NAudio.Wave;

namespace MyPersonalDjGui
{
    public class playback
    {
        private IWavePlayer waveOutDevice;
        private AudioFileReader audioStream; // Resolves the CS0103 error

        public void start(string filePath)
        {
            Stop(); // Always flush old devices before allocation

            try
            {
                waveOutDevice = new WaveOutEvent();
                audioStream = new AudioFileReader(filePath);
                waveOutDevice.Init(audioStream);
                waveOutDevice.Play();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization failed: " + ex.Message);
            }
        }

        public void pause()
        {
            waveOutDevice?.Pause();
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
            }
            catch { }
        }

        // ── TIME & SEEKING ENGINES (Resolves CS1061 Errors) ──

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
                audioStream.CurrentTime = TimeSpan.FromSeconds(seconds);
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