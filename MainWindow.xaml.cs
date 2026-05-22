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

namespace MyPersonalDjGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        songmenu myMenu = new songmenu();
        private MyPersonalDj.playback _playback = new MyPersonalDj.playback();
        public MainWindow()
        {
            InitializeComponent();

            // Load songs from the user's Music folder before binding the list
            try
            {
                var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                myMenu.LoadSongs(musicPath);
            }
            catch
            {
                // ignore failures to load; UI will simply show an empty list
            }

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
                    // No songs found in Music folder — try the configured ConsoleSongsFolder before launching console
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
                        // still no songs — do not launch console mode when running the GUI
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
                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyPersonalDj_console.log"), $"Play error: {ex}\n"); } catch { }
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
        }

        private void SongListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayButton_Click(sender, new RoutedEventArgs());
        }

        // Console log watcher functionality removed - no UI log box
    }

}