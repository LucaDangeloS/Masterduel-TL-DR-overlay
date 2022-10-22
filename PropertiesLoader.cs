using Newtonsoft.Json;

public sealed class PropertiesLoader
{
    private readonly static PropertiesLoader _instance = new();
    private readonly string PropertiesFileName = "Properties.conf";
    public PropertiesC Properties { get; init; }

    private PropertiesLoader()
    {
        PropertiesC prop;

        try
        {
            // Check if properties file exists, if not create a new one
            if (!File.Exists(PropertiesFileName))
            {
                throw new FileNotFoundException();
            }

            // Load properties from file
            var data = new Dictionary<string, string>();
            using (StreamReader r = new StreamReader(PropertiesFileName))
            {
                string json = r.ReadToEnd();
                dynamic jsonArray = JsonConvert.DeserializeObject(json);
                foreach (Newtonsoft.Json.Linq.JProperty key in jsonArray)
                {
                    data.Add(key.Name, (string) key.Value);
                }
            }

            prop = new PropertiesC()
            {
                MAX_PIXELS_DIFF = int.Parse(data["MAX_PIXELS_DIFF"]),
                SPLASH_SIZE = int.Parse(data["SPLASH_SIZE"]),
                COMPARISON_PRECISION = float.Parse(data["COMPARISON_PRECISION"]),
                MAX_POSSIBLE_CARDS_FROM_API = int.Parse(data["MAX_POSSIBLE_CARDS_FROM_API"]),
                MAX_CACHE_SIZE = int.Parse(data["MAX_CACHE_SIZE"])
            };
            if (!PropertyValidator(prop))
            {
                throw new InvalidDataException();
            }
        }
        catch (Exception)
        {
            try {
                File.Delete(PropertiesFileName);
            } catch (FileNotFoundException) { }

            prop = new PropertiesC();
            var str = JsonConvert.SerializeObject(prop.Serialize(), Formatting.Indented);
            File.WriteAllText(PropertiesFileName, str);
        }

        Properties = prop;
    }

    public static PropertiesLoader Instance
    {
        get
        {
            return _instance;
        }
    }

    public sealed class PropertiesC
    {
        public int MAX_PIXELS_DIFF { get; init; } = 25;
        public int SPLASH_SIZE { get; init; } = 24;
        public float COMPARISON_PRECISION { get; init; } = 0.96f;
        public int MAX_POSSIBLE_CARDS_FROM_API { get; init; } = 5;
        public int MAX_CACHE_SIZE { get; set; } = 40;

        public Dictionary<string, string> Serialize()
        {
            return new Dictionary<string, string>()
            {
                { "MAX_PIXELS_DIFF", MAX_PIXELS_DIFF.ToString() },
                { "SPLASH_SIZE", SPLASH_SIZE.ToString() },
                { "COMPARISON_PRECISION", COMPARISON_PRECISION.ToString() },
                { "MAX_POSSIBLE_CARDS_FROM_API", MAX_POSSIBLE_CARDS_FROM_API.ToString() },
                { "MAX_CACHE_SIZE", MAX_CACHE_SIZE.ToString() }
            };
        }
    }

    // private methods
    private bool PropertyValidator(PropertiesC prop)
    {
        return prop.SPLASH_SIZE > 0 &&
            prop.MAX_PIXELS_DIFF >= 0 &&
            prop.COMPARISON_PRECISION > 0.0f &&
            prop.COMPARISON_PRECISION <= 1.0 &&
            prop.MAX_POSSIBLE_CARDS_FROM_API > 0 &&
            prop.MAX_CACHE_SIZE > 0;
    }
}
