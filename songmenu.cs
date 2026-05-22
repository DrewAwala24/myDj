using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Supabase;

namespace MyPersonalDjGui
{
    internal class songmenu
    {
        private List<playlist> myRecord = new List<playlist>();
        private Client Supabase;

       
        public void LoadSongs(string folderpath)
        {
            string[] files = Directory.GetFiles(folderpath, "*.mp3");
            foreach (string file in files)
            {
                playlist newSong = new playlist(Path.GetFileNameWithoutExtension(file), file);

                myRecord.Add(newSong);
            }
        }
        public void displayLibrary()
        {
            Console.WriteLine("---------YOUR SONGS--------");
            for (int i = 0; i < myRecord.Count; i++)
            {
                Console.WriteLine($"{i}: {myRecord[i].getSongTitle()}");
            }
        }
        public playlist GetPlaylist(int choice)
        {
            if (choice >= 0 && choice < myRecord.Count)
            {
                return myRecord[choice];
            }
            return null;
        }

        internal int GetSongCount()
        {
            return myRecord.Count;
        }
        public List<string> getLibrary()
        {
            List<string> songs = new List<string>();
            foreach (var p in myRecord)
            {
                songs.Add(p.getSongTitle());
            }
            return songs;
        }

        internal void playSong(string? selected)
        {
            throw new NotImplementedException();
        }

  
        }
    }

