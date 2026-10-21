using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Python.Game;

namespace Python.Screens
{
    // Shows the saved top 10 as a list.
    public partial class HighscoresControl : UserControl
    {
        public HighscoresControl()
        {
            InitializeComponent();
            Load();
        }

        private MainWindow Main() { return (MainWindow)Application.Current.MainWindow; }

        private void Load()
        {
            List<HighscoreEntry> entries = Highscores.Load();
            List<string> lines = new List<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                string name = entries[i].Name;
                if (name.Length > 12) name = name.Substring(0, 12);
                lines.Add((i + 1) + ".  " + name.PadRight(13) + entries[i].Score);
            }
            ScoreList.ItemsSource = lines;
            EmptyText.Visibility = lines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e) { Main().ShowMenu(); }
    }
}
