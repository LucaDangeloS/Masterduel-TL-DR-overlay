using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TLDROverlay.Config.ConfigLoader;

namespace TLDROverlay;

public sealed class Logger
{
    public TextBox? ConsoleLog { get; set; }
    private bool LogToFile = false;
    private string LogFilePath = "log/log.txt";
    private static readonly Logger _logger = new ();

    public static Logger GetLogger()
    {
        return _logger;
    }

    private Logger()
    {
    }

    public void WriteToConsole(DateTime time, string message)
    {
        if (ConsoleLog != null)
        {
            ConsoleLog.Text += $"[{time:HH:mm:ss}] {message} {Environment.NewLine}";
            ConsoleLog.SelectionStart = ConsoleLog.Text.Length;
            ConsoleLog.ScrollToCaret();
        }
    }

    public void WriteToConsole(string message)
    {
        WriteToConsole(DateTime.Now, message);
    }
}

