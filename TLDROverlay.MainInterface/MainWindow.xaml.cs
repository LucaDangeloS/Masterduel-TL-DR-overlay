using System.Diagnostics;
using System.Windows;

namespace TLDROverlay.Interface
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
