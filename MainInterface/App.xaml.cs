using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TLDR_Masterduel_Overlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// https://www.youtube.com/watch?v=2FPFgW0xVB0
    /// https://www.youtube.com/watch?v=fZxZswmC_BY
    /// </summary>
    public partial class App : Application
    {
        //private volatile PropertiesC Properties = PropertiesLoader.Instance.Properties;
        //private readonly Logger _logger = Logger.GetLogger();
        //private CardInfo? lastCardSeen = null;
        //private readonly OCR ocr = new();
        protected int testInt;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

}
