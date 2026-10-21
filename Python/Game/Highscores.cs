using System;
using System.Collections.Generic;
using System.IO;

namespace Python.Game
{
    public class HighscoreEntry
    {
        public string Name;
        public int Score;
        public HighscoreEntry(string name, int score) { Name = name; Score = score; }
    }

    // Stores the top 10 in a text file under AppData, one "name;score" per line.
    public static class Highscores
    {
        private static string FilePath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PythonGame");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "highscores.txt");
        }

        public static List<HighscoreEntry> Load()
        {
            List<HighscoreEntry> list = new List<HighscoreEntry>();
            string path = FilePath();
            if (!File.Exists(path)) return list;

            foreach (string line in File.ReadAllLines(path))
            {
                string[] parts = line.Split(';');
                if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                    list.Add(new HighscoreEntry(parts[0], score));
            }
            list.Sort((a, b) => b.Score.CompareTo(a.Score));
            return list;
        }

        public static void Add(string name, int score)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Speler";
            name = name.Replace(";", " ").Trim(); // ';' is the field separator, keep it out of names

            List<HighscoreEntry> list = Load();
            list.Add(new HighscoreEntry(name, score));
            list.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (list.Count > 10) list = list.GetRange(0, 10);

            List<string> lines = new List<string>();
            foreach (HighscoreEntry e in list) lines.Add(e.Name + ";" + e.Score);
            File.WriteAllLines(FilePath(), lines);
        }
    }
}
