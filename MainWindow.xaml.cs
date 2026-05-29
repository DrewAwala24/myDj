using MyPersonalDj;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyPersonalDjGui
{
    public partial class MainWindow : Window
    {
        // ── CORE ENGINES & DATA STRUCTURES ──
        songmenu myMenu = new songmenu();
        private MyPersonalDjGui.playback _playback = new MyPersonalDjGui.playback();
        private DispatcherTimer uiSyncTimer;
        private bool isUserDraggingSlider = false;

        public ObservableCollection<string> ConsoleActivityLogs { get; set; }
        private bool isConsoleLayerActive = false;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize and bind the green matrix logging engine
            ConsoleActivityLogs = new ObservableCollection<string>();
            ConsoleLogListBox.ItemsSource = ConsoleActivityLogs;

            // Setup real-time track tracking ticker
            uiSyncTimer = new DispatcherTimer();
            uiSyncTimer.Interval = TimeSpan.FromMilliseconds(500);
            uiSyncTimer.Tick += UiSyncTimer_Tick;
            uiSyncTimer.Start();

            LogSystemMessage("INIT", "Futaba Palace Core UI online.");

            // Attempt default system path discovery scanning
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
                SongCountLabel.Text = $"{count} songs";

                if (count > 0)
                {
                    SongListBox.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void LoadSongs()
        {
            var songs = myMenu.getLibrary();
            SongListBox.ItemsSource = songs;
        }

        // ── PERSISTENT PALACE LAYER ANIMATION TIMELINE TRANSITIONER ──
        private void ShiftViewportTheme(bool focusConsole)
        {
            if (focusConsole == isConsoleLayerActive) return;

            string storyboardResourceName = focusConsole ? "MoveToConsoleStoryboard" : "MoveToIdleStoryboard";
            Storyboard targetStoryboard = (Storyboard)this.FindResource(storyboardResourceName);

            if (targetStoryboard != null)
            {
                targetStoryboard.Begin();
                isConsoleLayerActive = focusConsole;
                ToggleConsoleViewBtn.Content = focusConsole ? "ALBUM" : "LOGS";

                if (focusConsole)
                {
                    LogSystemMessage("NAVIGATOR", "Analyzing grid structures... Accessing database keys.");
                }
            }
        }

        private void LogSystemMessage(string header, string body)
        {
            string timeMarker = DateTime.Now.ToString("HH:mm:ss.fff");
            ConsoleActivityLogs.Add($"[{timeMarker}] {header.ToUpper()} >> {body}");

            // Limit list size to preserve rendering memory blocks
            if (ConsoleActivityLogs.Count > 20) ConsoleActivityLogs.RemoveAt(0);

            // Automate jumping viewport index to trailing edge node lines
            if (ConsoleLogListBox.Items.Count > 0)
            {
                ConsoleLogListBox.ScrollIntoView(ConsoleLogListBox.Items[ConsoleLogListBox.Items.Count - 1]);
            }
        }

        // ── MEDIA CONTROL DECK COMMAND HOOKS ──
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
                        if (!System.IO.File.Exists(path)) return;

                        _playback.start(path);
                        StatusText.Text = $"Playing: {playlistItem.getSongTitle()}";
                        LogSystemMessage("PLAY", $"Access allocated memory block for track: {playlistItem.getSongTitle().ToUpper()}");

                        // Spin up the vinyl animation disk
                        Storyboard spinStoryboard = (Storyboard)this.FindResource("SpinVinylStoryboard");
                        spinStoryboard?.Begin();

                        // Cleanly slide back up to Morgana view after initialization processing sequences complete
                        Task.Delay(1800).ContinueWith(_ =>
                        {
                            Dispatcher.Invoke(() => {
                                if (isConsoleLayerActive) ShiftViewportTheme(focusConsole: false);
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        LogSystemMessage("CRIT", $"Playback error: {ex.Message}");
                    }
                }
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _playback.pause();
            LogSystemMessage("STREAM", "Audio device state suspended.");

            Storyboard spinStoryboard = (Storyboard)this.FindResource("SpinVinylStoryboard");
            spinStoryboard?.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _playback.Stop();
            StatusText.Text = "Stopped";
            SeekBar.Value = 0;
            TimeElapsed.Text = "0:00";
            LogSystemMessage("HALT", "Flushing runtime pipeline blocks.");

            Storyboard spinStoryboard = (Storyboard)this.FindResource("SpinVinylStoryboard");
            spinStoryboard?.Stop();

            ShiftViewportTheme(focusConsole: false);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            var total = myMenu.GetSongCount();
            if (total == 0) return;

            // Instantly transition out into character tracking timeline sequence frames
            ShiftViewportTheme(focusConsole: true);

            int idx = _playback.GetShuffleIndex(total);
            var item = myMenu.GetPlaylist(idx);
            if (item != null)
            {
                _playback.start(item.getFilePath());
                SongListBox.SelectedIndex = idx;

                LogSystemMessage("SHUFFLE", $"Random map target slot index resolved: [{idx}]");
                LogSystemMessage("EXEC_LOAD", $"Streaming file node: {System.IO.Path.GetFileName(item.getFilePath()).ToUpper()}");

                Storyboard spinStoryboard = (Storyboard)this.FindResource("SpinVinylStoryboard");
                spinStoryboard?.Begin();

                // Automate slide recovery back to record deck layout configuration rulesets
                Task.Delay(2500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => {
                        if (isConsoleLayerActive) ShiftViewportTheme(focusConsole: false);
                    });
                });
            }
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

        // ── LIST COMPONENT SUBSCRIPTION BINDINGS ──
        private void SongListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
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

        // ── DYNAMIC TIME TRACKER SYNC SYNCERS ──
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
                    LogSystemMessage("SEEK", $"Pipeline sequence adjusted to: {targetSeconds:F2}s");
                }
                catch { }
            }
            isUserDraggingSlider = false;
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_playback != null && sender is Slider volSlider)
            {
                float calculatedVolume = (float)(volSlider.Value / 100.0);
                _playback.SetVolume(calculatedVolume);
            }
        }

        // ── DISK IO MOUNT STRATEGIES ──
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
                    ShiftViewportTheme(focusConsole: true);
                    LogSystemMessage("FS_MOUNT", $"Accessing path: {folder}");

                    myMenu.LoadSongs(folder);
                    LoadSongs();
                    var count = myMenu.GetSongCount();
                    SongCountLabel.Text = $"{count} songs";
                    StatusText.Text = $"Loaded {count} songs from {folder}";

                    LogSystemMessage("FS_INDEX", $"Successfully cataloged {count} active streaming nodes.");
                    if (count > 0) SongListBox.SelectedIndex = 0;
                }
                catch (Exception ex) { LogSystemMessage("FS_FAULT", ex.Message); }
            }
        }

        private void ToggleConsoleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ShiftViewportTheme(focusConsole: !isConsoleLayerActive);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try { _playback?.Stop(); } catch { }
            try { uiSyncTimer?.Stop(); } catch { }
        }
    }
}