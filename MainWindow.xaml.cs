using MyPersonalDj;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MyPersonalDjGui
{
    public partial class MainWindow : Window
    {
        songmenu myMenu = new songmenu();
        private MyPersonalDjGui.playback _playback = new MyPersonalDjGui.playback();
        private DispatcherTimer uiSyncTimer;
        private bool isUserDraggingSlider = false;
        private bool isConsoleLayerActive = false;

        public ObservableCollection<string> ConsoleActivityLogs { get; set; }

        private enum RepeatMode { Off, One, All }
        private RepeatMode currentRepeatMode = RepeatMode.Off;

        public MainWindow()
        {
            InitializeComponent();

            ConsoleActivityLogs = new ObservableCollection<string>();
            ConsoleLogListBox.ItemsSource = ConsoleActivityLogs;

            uiSyncTimer = new DispatcherTimer();
            uiSyncTimer.Interval = TimeSpan.FromMilliseconds(500);
            uiSyncTimer.Tick += UiSyncTimer_Tick;
            uiSyncTimer.Start();

            Storyboard scanline = (Storyboard)this.FindResource("ScanlineFlickerStoryboard");
            scanline?.Begin();

            LogSystemMessage("INIT", "Futaba Core Engine active. Assets mapped cleanly.");
            LoadDefaultLibrary();
        }

        private void LoadDefaultLibrary()
        {
            try
            {
                var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                myMenu.LoadSongs(musicPath);
                LoadSongsToList();
                StatusText.Text = $"Songs loaded: {myMenu.GetSongCount()}";
                SongCountLabel.Text = $"{myMenu.GetSongCount()} songs";
            }
            catch { }
        }

        private void LoadSongsToList() => SongListBox.ItemsSource = myMenu.getLibrary();

        private void LogSystemMessage(string header, string body)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            ConsoleActivityLogs.Add($"[{time}] {header.ToUpper()} >> {body}");
            if (ConsoleActivityLogs.Count > 20) ConsoleActivityLogs.RemoveAt(0);
            if (ConsoleLogListBox.Items.Count > 0)
                ConsoleLogListBox.ScrollIntoView(ConsoleLogListBox.Items[ConsoleLogListBox.Items.Count - 1]);
        }

        private void ShiftViewportTheme(bool focusConsole)
        {
            if (focusConsole == isConsoleLayerActive) return;
            string key = focusConsole ? "MoveToConsoleStoryboard" : "MoveToIdleStoryboard";
            Storyboard sb = (Storyboard)this.FindResource(key);
            if (sb != null)
            {
                sb.Begin();
                isConsoleLayerActive = focusConsole;
                ToggleConsoleViewBtn.Content = focusConsole ? "ALBUM" : "LOGS";

                if (focusConsole) { FutabaIdleVideo.Pause(); FutabaHackingVideo.Play(); FutabaGlanceVideo.Play(); }
                else { FutabaHackingVideo.Pause(); FutabaGlanceVideo.Pause(); FutabaIdleVideo.Play(); }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongListBox.SelectedIndex >= 0)
            {
                var item = myMenu.GetPlaylist(SongListBox.SelectedIndex);
                if (item != null && File.Exists(item.getFilePath()))
                {
                    _playback.start(item.getFilePath());

                    // ── STRING CLEANING DECK ──
                    string polishedTitle = item.getSongTitle();
                    polishedTitle = Regex.Replace(polishedTitle, @"\[.*?\]", "").Trim(); // Strips out YouTube hashes [rNkBob...]
                    polishedTitle = Regex.Replace(polishedTitle, @"\s*([\(\[][^)]*audio[^)]*[\)\]])", "", RegexOptions.IgnoreCase).Trim(); // Clean (Official Audio)
                    polishedTitle = Regex.Replace(polishedTitle, @"\s*([\(\[][^)]*video[^)]*[\)\]])", "", RegexOptions.IgnoreCase).Trim(); // Clean (Official Video)
                    polishedTitle = polishedTitle.Replace("_", " "); // Clear structural underscores

                    SongTitleText.Text = polishedTitle;
                    LogSystemMessage("PLAY", $"Streaming: {polishedTitle}");

                    FutabaIdleVideo.Play();
                    FutabaGlanceVideo.Play();
                    FutabaHackingVideo.Play();

                    Task.Delay(2000).ContinueWith(_ => Dispatcher.Invoke(() => {
                        if (isConsoleLayerActive) ShiftViewportTheme(false);
                    }));
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _playback.Stop();
            FutabaIdleVideo.Stop();
            FutabaHackingVideo.Stop();
            FutabaGlanceVideo.Stop();
            LogSystemMessage("HALT", "Playback terminated.");
            ShiftViewportTheme(false);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            int total = myMenu.GetSongCount();
            if (total == 0) return;
            ShiftViewportTheme(true);
            int idx = _playback.GetShuffleIndex(total);
            SongListBox.SelectedIndex = idx;
            PlayButton_Click(null, null);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            switch (currentRepeatMode)
            {
                case RepeatMode.Off:
                    currentRepeatMode = RepeatMode.One;
                    RepeatButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF66"));
                    LogSystemMessage("LOOP", "Mode: REPEAT_ONE");
                    break;
                case RepeatMode.One:
                    currentRepeatMode = RepeatMode.All;
                    RepeatButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8001A"));
                    LogSystemMessage("LOOP", "Mode: REPEAT_ALL");
                    break;
                case RepeatMode.All:
                    currentRepeatMode = RepeatMode.Off;
                    RepeatButton.Foreground = Brushes.White;
                    LogSystemMessage("LOOP", "Mode: OFF");
                    break;
            }
        }

        private void UiSyncTimer_Tick(object sender, EventArgs e)
        {
            if (!isUserDraggingSlider)
            {
                double cur = _playback.GetCurrentTimeInSeconds();
                double tot = _playback.GetTotalTimeInSeconds();
                if (tot > 0)
                {
                    SeekBar.Maximum = tot;
                    SeekBar.Value = cur;
                    TimeElapsed.Text = TimeSpan.FromSeconds(cur).ToString(@"m\:ss");
                    TimeTotal.Text = TimeSpan.FromSeconds(tot).ToString(@"m\:ss");

                    if (cur >= tot - 0.5) HandleSongFinished();
                }
            }
        }

        private void HandleSongFinished()
        {
            if (currentRepeatMode == RepeatMode.One) PlayButton_Click(null, null);
            else NextButton_Click(null, null);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int next = SongListBox.SelectedIndex + 1;
            if (next >= myMenu.GetSongCount()) next = (currentRepeatMode == RepeatMode.All) ? 0 : next - 1;
            SongListBox.SelectedIndex = next;
            PlayButton_Click(null, null);
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            int prev = SongListBox.SelectedIndex - 1;
            if (prev < 0) prev = 0;
            SongListBox.SelectedIndex = prev;
            PlayButton_Click(null, null);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _playback?.SetVolume((float)(e.NewValue / 100.0));
        }

        private void SeekBar_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { isUserDraggingSlider = true; }
        private void SeekBar_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _playback.SetPositionInSeconds(SeekBar.Value);
            isUserDraggingSlider = false;
        }

        private void Video_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement me) { me.Position = TimeSpan.Zero; me.Play(); }
        }

        private void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Select Folder", CheckFileExists = false, FileName = "Folder Selection" };
            if (dialog.ShowDialog() == true)
            {
                string path = Path.GetDirectoryName(dialog.FileName);
                myMenu.LoadSongs(path);
                LoadSongsToList();
            }
        }

        private void ToggleConsoleViewBtn_Click(object sender, RoutedEventArgs e) => ShiftViewportTheme(!isConsoleLayerActive);
        private void SongListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void SongListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => PlayButton_Click(null, null);
        protected override void OnClosed(EventArgs e) { _playback.Stop(); base.OnClosed(e); }
    }
}