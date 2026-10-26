using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Python.Game;

namespace Python.Screens
{
    // Reads the current settings when opened and writes them back to the shared
    // GameSettings on "Terug".
    public partial class SettingsControl : UserControl
    {
        private string _skin; // chosen snake colour, not saved until Back

        public SettingsControl()
        {
            InitializeComponent();
            LoadFromSettings();
        }

        private MainWindow Main() { return (MainWindow)Application.Current.MainWindow; }

        private void LoadFromSettings()
        {
            GameSettings s = Main().Settings;

            DiffMakkelijk.IsChecked = s.Difficulty == Difficulty.Makkelijk;
            DiffMoeilijk.IsChecked = s.Difficulty == Difficulty.Moeilijk;

            Lvl1.IsChecked = s.Level == 1;
            Lvl2.IsChecked = s.Level == 2;
            Lvl3.IsChecked = s.Level == 3;

            TopicTalen.IsChecked = s.Topic == Topic.Programmeertalen;
            TopicWiskunde.IsChecked = s.Topic == Topic.Wiskunde;

            _skin = s.SkinColor;
            HighlightSkin();
        }

        private void Skin_Click(object sender, RoutedEventArgs e)
        {
            _skin = (string)((Button)sender).Tag;
            HighlightSkin();
        }

        // White border around the selected skin.
        private void HighlightSkin()
        {
            foreach (object child in SkinPanel.Children)
            {
                Button b = child as Button;
                if (b == null) continue;
                bool selected = (string)b.Tag == _skin;
                b.BorderThickness = new Thickness(selected ? 3 : 0);
                b.BorderBrush = Brushes.White;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            GameSettings s = Main().Settings;

            s.Difficulty = DiffMoeilijk.IsChecked == true ? Difficulty.Moeilijk : Difficulty.Makkelijk;

            if (Lvl3.IsChecked == true) s.Level = 3;
            else if (Lvl2.IsChecked == true) s.Level = 2;
            else s.Level = 1;

            s.Topic = TopicWiskunde.IsChecked == true ? Topic.Wiskunde : Topic.Programmeertalen;
            s.SkinColor = _skin;

            Main().ShowMenu();
        }
    }
}
