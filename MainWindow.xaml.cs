using MyPersonalDj;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyPersonalDjGui
{
    public partial class MainWindow : Window
    {
        songmenu myMenu = new songmenu();
        private MyPersonalDjGui.playback _playback = new MyPersonalDjGui.playback();
        private DispatcherTimer uiSyncTimer;
        private bool isUserDraggingSlider = false;

        public MainWindow()
        {
            InitializeComponent();

            uiSyncTimer = new DispatcherTimer();
            uiSyncTimer.Interval = TimeSpan.FromMilliseconds(500);
            uiSyncTimer.Tick += UiSyncTimer_Tick;
            uiSyncTimer.Start();

            try
            {
                var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                myMenu.LoadSongs(musicPath);
            }
            catch { }

            LoadSongs();

            try
            {
                var count = myMenu.GetSongCount();
                StatusText.Text = $"Songs loaded: {count}";

                if (count > 0)
                {
                    SongListBox.SelectedIndex = 0;
                }
                else
                {
                    var consoleFolder = Config.ConsoleSongsFolder;
                    StatusText.Text = $"No songs found in Music folder — trying configured folder: {consoleFolder}";
                    try
                    {
                        myMenu.LoadSongs(consoleFolder);
                        LoadSongs();
                    }
                    catch { }

                    var newCount = myMenu.GetSongCount();
                    if (newCount > 0)
                    {
                        StatusText.Text = $"Songs loaded from configured folder: {consoleFolder} ({newCount})";
                        SongListBox.SelectedIndex = 0;
                    }
                    else
                    {
                        StatusText.Text = $"No songs found in Music or configured folder ({consoleFolder}).";
                    }
                }
            }
            catch { }
        }

        private void LoadSongs()
        {
            var songs = myMenu.getLibrary();
            SongListBox.ItemsSource = songs;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongListBox.SelectedIndex >= 0)
            {
                var playlistItem = myMenu.GetPlaylist(SongListBox.SelectedIndex);
                if (playlistItem != null)
                {
                    try
                    {
                        var path = playlistItem.getFilePath();
                        if (!System.IO.File.Exists(path))
                        {
                            StatusText.Text = "File not found: " + path;
                            return;
                        }
                        _playback.start(path);
                        StatusText.Text = $"Playing: {playlistItem.getSongTitle()}";
                    }
                    catch (Exception ex)
                    {
                        StatusText.Text = "Playback error: " + ex.Message;
                    }
                }
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _playback.pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _playback.Stop();
            StatusText.Text = "Stopped";
            SeekBar.Value = 0;
            TimeElapsed.Text = "0:00";
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            var total = myMenu.GetSongCount();
            if (total == 0) return;
            int idx = _playback.GetShuffleIndex(total);
            var item = myMenu.GetPlaylist(idx);
            if (item != null)
            {
                _playback.start(item.getFilePath());
                StatusText.Text = $"Shuffled to: {item.getSongTitle()}";
                SongListBox.SelectedIndex = idx;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try { _playback?.Stop(); } catch { }
            try { uiSyncTimer?.Stop(); } catch { }
        }

        private void SongListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlayButton_Click(sender, new RoutedEventArgs());
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            var total = myMenu.GetSongCount();
            if (total == 0) return;
            int idx = SongListBox.SelectedIndex - 1;
            if (idx < 0) idx = total - 1;
            SongListBox.SelectedIndex = idx;
            PlayButton_Click(sender, new RoutedEventArgs());
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var total = myMenu.GetSongCount();
            if (total == 0) return;
            int idx = SongListBox.SelectedIndex + 1;
            if (idx >= total) idx = 0;
            SongListBox.SelectedIndex = idx;
            PlayButton_Click(sender, new RoutedEventArgs());
        }

        private void SongListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongListBox.SelectedIndex < 0) return;
            var item = myMenu.GetPlaylist(SongListBox.SelectedIndex);
            if (item == null) return;
            SongTitleText.Text = item.getSongTitle();
            ArtistText.Text = System.IO.Path.GetFileName(item.getFilePath());
        }

        // ── TRACK CLOCK AND PROGRESS SYNC ENGINE ──

        private void UiSyncTimer_Tick(object sender, EventArgs e)
        {
            if (_playback != null && !isUserDraggingSlider)
            {
                try
                {
                    double currentSecs = _playback.GetCurrentTimeInSeconds();
                    double totalSecs = _playback.GetTotalTimeInSeconds();

                    if (totalSecs > 0)
                    {
                        SeekBar.Maximum = totalSecs;
                        SeekBar.Value = currentSecs;

                        TimeElapsed.Text = TimeSpan.FromSeconds(currentSecs).ToString(@"m\:ss");
                        TimeTotal.Text = TimeSpan.FromSeconds(totalSecs).ToString(@"m\:ss");
                    }
                }
                catch { }
            }
        }

        private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isUserDraggingSlider = true;
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_playback != null)
            {
                try
                {
                    double targetSeconds = SeekBar.Value;
                    _playback.SetPositionInSeconds(targetSeconds);
                }
                catch { }
            }
            isUserDraggingSlider = false;
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Placeholder required by layout references
        }

        private void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a folder — pick any file inside it",
                Filter = "MP3 Files|*.mp3",
                CheckFileExists = false
            };
            if (dialog.ShowDialog() == true)
            {
                var folder = System.IO.Path.GetDirectoryName(dialog.FileName);
                try
                {
                    myMenu.LoadSongs(folder);
                    LoadSongs();
                    var count = myMenu.GetSongCount();
                    SongCountLabel.Text = $"{count} songs";
                    StatusText.Text = $"Loaded {count} songs from {folder}";
                    if (count > 0) SongListBox.SelectedIndex = 0;
                }
                catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
            }
        }
    }
}