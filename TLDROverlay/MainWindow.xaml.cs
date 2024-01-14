using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TesseractOCR;
using TLDROverlay.Engine;

namespace TLDROverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MasterduelEngine _engine = new MasterduelEngine(0.9f);
        private readonly Logger _logger = Logger.GetLogger();
        private readonly TextBox _consoleLogger;

        public MainWindow()
        {
            InitializeComponent();
            _consoleLogger = ConsoleLog;
            _logger.AddLogHook((string message) =>
            {
                _consoleLogger.Dispatcher.Invoke(() =>
                {
                    _consoleLogger.AppendText(message + "\n");
                    _consoleLogger.ScrollToEnd();
                });
            });
        }

        private void Start_click(object sender, RoutedEventArgs e)
        {
            // disable start button
            StartButton.IsEnabled = false;

            try
            {
                _engine.StartLoop();
                _logger.Log("Started", LogLevel.Information);
            } catch (Exception ex)
            {
                _logger.Log(ex.Message, LogLevel.Error);
                StartButton.IsEnabled = true;
                return;
            }
            StopButton.IsEnabled = true;
        }

        private void Stop_click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            _engine.StopLoop();
            _logger.Log("Stopped", LogLevel.Information);
            StartButton.IsEnabled = true;
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
            _engine.SetMemoryCaching(MemCacheCB.IsChecked == true);

            if (MemCacheCB.IsChecked == true)
            {
                _logger.Log("Temporal Cache: Enabled");
            }
            else
            {
                _logger.Log("Temporal Cache: Disabled");
            }
        }

        private void TogglePersistentCacheCheckBox(object sender, RoutedEventArgs e)
        {
            _engine.SetDBCaching(PersistentCacheCB.IsChecked == true);

            if (PersistentCacheCB.IsChecked == true)
            {
                _logger.Log("Persistent Cache: Enabled");
            }
            else
            {
                _logger.Log("Persistent Cache: Disabled");
            }
        }

        private void ClearMemCache(object sender, RoutedEventArgs e)
        {
            _logger.Log("Clearing Temporal cache");
        }

        private void ClearPersistentCache(object sender, RoutedEventArgs e)
        {
            _logger.Log("Clearing Persistent cache");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearMemCacheButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
