using Newtonsoft.Json;
using SQLite;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;

namespace TLDROverlay.Config;

public sealed class ConfigLoader
{
    private readonly static ConfigLoader _instance = new();
    private readonly Logger _logger = Logger.GetLogger();
    private readonly DefaultConfig _default = new();
    private NameValueCollection _config = new();
    private readonly static string FloatFormat = "0.00";

    private ConfigLoader()
    {
        _config = ConfigurationManager.AppSettings;

        if (!ValidateConfiguration())
        {
            _logger.WriteToConsole("Configuration file not valid, using default values.");
            _config = _default.Serialize();
        }
    }

    public static ConfigLoader Instance
    {
        get
        {
            return _instance;
        }
    }

    // Getters
    public float GetFloatProperty(string propertyName)
    {
        string? prop = _config[propertyName];
        if (prop == null) throw new ArgumentException($"The property '{propertyName}' does not exist.");
        return float.Parse(prop);
    }

    public int GetIntProperty(string propertyName)
    {
        string? prop = _config[propertyName];
        if (prop == null) throw new ArgumentException($"The property '{propertyName}' does not exist.");
        return int.Parse(prop);
    }

    // Setters
    public void SetFloatProperty(string key, float value)
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        float prevValue = GetFloatProperty(key);
        config.AppSettings.Settings.Add(key, value.ToString(FloatFormat));
        if (!ValidateConfiguration())
        {
            config.AppSettings.Settings.Add(key, prevValue.ToString(FloatFormat));
            throw new ArgumentException($"The property '{key}' can't take the value '{value}'");
        }
    }

    public void SetIntProperty(string key, int value)
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        int prevValue = GetIntProperty(key);
        config.AppSettings.Settings.Add(key, value.ToString());
        if (!ValidateConfiguration())
        {
            config.AppSettings.Settings.Add(key, prevValue.ToString());
            throw new ArgumentException($"The property '{key}' can't take the value '{value}'");
        }
    }

    // private methods
    private sealed class DefaultConfig
    {
        private int D_MAX_PIXELS_DIFF { get; init; } = 20;
        private int D_SPLASH_SIZE { get; init; } = 24;
        private float D_COMPARISON_PRECISION { get; init; } = 0.98f;
        private int D_MAX_POSSIBLE_CARDS_FROM_API { get; init; } = 2;
        private int D_MAX_CACHE_ENTRIES { get; set; } = 40;

        public NameValueCollection Serialize()
        {
            return new NameValueCollection()
            {
                { ConfigMappings.MAX_PIXELS_DIFF,               D_MAX_PIXELS_DIFF.ToString() },
                { ConfigMappings.SPLASH_SIZE,                   D_SPLASH_SIZE.ToString() },
                { ConfigMappings.COMPARISON_PRECISION,          D_COMPARISON_PRECISION.ToString(FloatFormat) },
                { ConfigMappings.MAX_POSSIBLE_CARDS_FROM_API,   D_MAX_POSSIBLE_CARDS_FROM_API.ToString() },
                { ConfigMappings.MAX_CACHE_ENTRIES,             D_MAX_CACHE_ENTRIES.ToString() }
            };
        }
    }

    private bool ValidateConfiguration()
    {
        try
        {
            return GetIntProperty(ConfigMappings.SPLASH_SIZE) > 0 &&
                GetIntProperty(ConfigMappings.MAX_PIXELS_DIFF) >= 0 &&
                GetFloatProperty(ConfigMappings.COMPARISON_PRECISION) > 0.0f &&
                GetFloatProperty(ConfigMappings.COMPARISON_PRECISION) <= 1.0 &&
                GetIntProperty(ConfigMappings.MAX_POSSIBLE_CARDS_FROM_API) > 0 &&
                GetIntProperty(ConfigMappings.MAX_CACHE_ENTRIES) > 0;
        }
        catch (ArgumentException e)
        {
            return false;
        }
    }
}

