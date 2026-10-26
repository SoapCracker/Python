using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Python.Screens
{
    public partial class InfoScreenControl : UserControl
    {
        public InfoScreenControl()
        {
            InitializeComponent();
        }

        // Helper om naar het hoofdmenu te gaan
        private MainWindow Main()
        {
            return (MainWindow)Application.Current.MainWindow;
        }

        // Terug-knop actie (komt overeen met Click="Back_Click" in de XAML)
        private void Back_ClickINFO(object sender, RoutedEventArgs e)
        {
            Main().ShowMenu();
        }
    }
}