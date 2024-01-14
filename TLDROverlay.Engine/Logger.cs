using Microsoft.Extensions.Logging;
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
    private bool LogToFile = false;
    private string LogFilePath = "log/log.txt";
    private static readonly Logger _logger = new ();
    // array of log hooks
    private List<Action<string>> _logHooks = new ();
    public LogLevel _logLevel { get; set; }

    public static Logger GetLogger()
    {
        return _logger;
    }

    private Logger()
    {
    }

    public void Log(string message, LogLevel logLevel = LogLevel.Debug)
    {
        if (logLevel < _logLevel)
        {
            return;
        }

        // timestamp
        message = DateTime.Now.ToString("HH:mm:ss") + " " + message;
        // loglevel string
        message = "[" + logLevel.ToString().ToUpper() + "] " + message;

        if (LogToFile)
        {
            System.IO.File.AppendAllText(LogFilePath, message + "\n");
        }
        else
        {
            Debug.WriteLine(message);
        }

        // call hooks
        foreach (var hook in _logHooks)
        {
            hook(message);
        }
    }

    public void AddLogHook(Action<string> hook)
    {
        _logHooks.Add(hook);
    }
}

