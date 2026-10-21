using System.Windows;
using Python.Game;
using Python.Screens;

namespace Python
{
    public partial class MainWindow : Window
    {
        // Chosen settings, shared with every screen.
        public GameSettings Settings = new GameSettings();

        public MainWindow()
        {
            InitializeComponent();
            ShowMenu();
        }

        public void ShowMenu() { Host.Content = new MenuControl(); }
        public void ShowSettings() { Host.Content = new SettingsControl(); }
        public void ShowGame() { Host.Content = new GameControl(); }
        public void ShowHighscores() { Host.Content = new HighscoresControl(); }
    }
}
