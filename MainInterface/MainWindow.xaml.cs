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

namespace TLDR_Masterduel_Overlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_click(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void Stop_click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ShowLogCheckBox(object sender, RoutedEventArgs e)
        {
            if (ShowLog.IsChecked == true)
            {
                ConsoleLog.Visibility = Visibility.Visible;
                Window.MinHeight = 300;
            }
            else
            {
                ConsoleLog.Visibility = Visibility.Collapsed;
                Window.MinHeight = 180;
                Window.Height = 180;
            }
        }
    }
}
