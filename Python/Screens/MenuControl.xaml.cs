using System.Windows;
using System.Windows.Controls;

namespace Python.Screens
{
    // The start menu. Each button asks the main window to show a different screen.
    public partial class MenuControl : UserControl
    {
        public MenuControl() { InitializeComponent(); }

        private MainWindow Main() { return (MainWindow)Application.Current.MainWindow; }

        private void Start_Click(object sender, RoutedEventArgs e) { Main().ShowGame(); }
        private void Highscores_Click(object sender, RoutedEventArgs e) { Main().ShowHighscores(); }
        private void Settings_Click(object sender, RoutedEventArgs e) { Main().ShowSettings(); }
        private void Quit_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
    }
}
