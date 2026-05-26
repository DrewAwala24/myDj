using System;
using System.Threading.Channels;

namespace MyPersonalDjGui
{
    class Program
    {
        public static void RunConsole(string[] args)
        {
            try
            {
                var log = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyPersonalDj_console.log");
                System.IO.File.AppendAllText(log, $"RunConsole invoked at {DateTime.Now}\n");
            }
            catch { }

            audioplayer myDj = new audioplayer();
            songmenu myMenu = new songmenu();

            string folderpath = MyPersonalDjGui.Config.ConsoleSongsFolder;
            myMenu.LoadSongs(folderpath);

           
            try
            {
                var log = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyPersonalDj_console.log");
                foreach (var p in typeof(songmenu).GetMethod("getLibrary").Invoke(myMenu, null) as System.Collections.Generic.List<string>)
                {
                    System.IO.File.AppendAllText(log, $"Found song: {p}{Environment.NewLine}");
                }
            }
            catch { }

            bool isRunning = true;
            playback remote = new playback();
            myMenu.displayLibrary();

            while (isRunning)
            {
                Console.WriteLine("[N] New Song, [P] Pause/Resume, [E] Exit, [S] Shuffle");
                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.N:
                        Console.WriteLine("Enter Song Number: ");
                        int choice = Convert.ToInt32(Console.ReadLine());
                        playlist selected = myMenu.GetPlaylist(choice);
                        if (selected != null) remote.start(selected.getFilePath());
                        break;

                    case ConsoleKey.P:
                        remote.pause();
                        break;

                    case ConsoleKey.S:
                        int total = myMenu.GetSongCount();
                        int randomIndex = remote.GetShuffleIndex(total);
                        playlist randomSongs = myMenu.GetPlaylist(randomIndex);

                        if (randomSongs != null)
                        {
                            remote.start(randomSongs.getFilePath());
                            Console.WriteLine($"\n>>> Shuffled to: {randomSongs.getSongTitle()}");
                        }
                        break;

                    case ConsoleKey.E:
                        Console.WriteLine("Closing App.........");
                        remote.Stop();
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Unkown Command! ");
                        Console.WriteLine("Use N, P, S, E");
                        break;
                }


            }

        }
    }
}