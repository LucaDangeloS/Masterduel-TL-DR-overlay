using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private void ToggleShowLogCheckBox(object sender, RoutedEventArgs e)
        {
            if (ShowLogCB.IsChecked == true)
            {
                ConsoleLog.Visibility = Visibility.Visible;
                Window.MinHeight = 330;
            }
            else
            {
                ConsoleLog.Visibility = Visibility.Collapsed;
                Window.MinHeight = 200;
                Window.Height = 200;
            }
        }

        private void ToggleMemoryCacheCheckBox(object sender, RoutedEventArgs e)
        {
            if (MemCacheCB.IsChecked == true)
            {
                Debug.WriteLine("Temporal Cache: Enabled");
            }
            else
            {
                Debug.WriteLine("Temporal Cache: Disabled");
            }
        }

        private void TogglePersistentCacheCheckBox(object sender, RoutedEventArgs e)
        {
            if (PersistentCacheCB.IsChecked == true)
            {
                Debug.WriteLine("Persistent Cache: Enabled");
            }
            else
            {
                Debug.WriteLine("Persistent Cache: Disabled");
            }
        }

        private void ClearMemCache(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Clearing Temporal cache");
        }

        private void ClearPersistentCache(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Clearing Persistent cache");
        }

    }
}
