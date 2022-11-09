using Masterduel_TLDR_overlay.Masterduel;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Ocr;
using System.Diagnostics;
using Masterduel_TLDR_overlay.WindowHandlers;
using static Masterduel_TLDR_overlay.WindowHandlers.WindowHandlerInterface;
using Masterduel_TLDR_overlay.Caching;
using Masterduel_TLDR_overlay.Windows;
using static Masterduel_TLDR_overlay.Screen.ImageProcessing;
using static Masterduel_TLDR_overlay.PropertiesLoader;
using Masterduel_TLDR_overlay.Exceptions;

namespace Masterduel_TLDR_overlay;

public partial class MainForm : System.Windows.Forms.Form
{
    private volatile PropertiesC Properties = PropertiesLoader.Instance.Properties;
    private readonly Logger _logger = Logger.GetLogger();
    private CardInfo? lastCardSeen = null;
    private readonly OCR ocr = new();
    private readonly bool _dbCaching = true;
    private readonly bool _memCaching = true;
    private readonly bool _skipDuelScreenCheck = false;
    private readonly bool _skipCardInScreenCheck = false;
    
    public MainForm()
    {
        InitializeComponent();
    }

    private void StartLoop(object sender, EventArgs e)
    {
        startButton.Enabled = false;
        stopButton.Enabled = true;

        WindowHandlerInterface handler;
        //bool mouseClicked;
        //var form = new OverlayForm();
        //form.SetWindowStyle();
        //form.Show();

        try
        {
            handler = new Handler(MasterduelWindow.WINDOW_NAME);
        }
        catch (NoWindowFoundException)
        {
            Invoke(new (() => _logger.WriteToConsole("No window found")));
            return;
        }

        MemCache cachedSplashes = new(Properties.MAX_PIXELS_DIFF, Properties.SPLASH_SIZE);
        LocalDB db = new();
        db.Initialize();

        new Thread(async () =>
        {
            DateTime start = DateTime.UtcNow;

            while (stopButton.Enabled)
            {
                // Get current window

                if (handler.IsWindowCurrentlySelected())
                {
                    //if (handler.GetLeftMousePressed())
                    //{
                    await DoThings(handler, cachedSplashes, db);
                    //}
                }
                Thread.Sleep(500);
            }
        }).Start();
    }

    private async Task DoThings(WindowHandlerInterface handler, MemCache cachedSplashes, LocalDB db)
    {
        (Point, Point) windowArea;
        try
        {
            windowArea = handler.GetWindowPoints();
        }
        catch (Exception)
        {
            return;
        }
        CardInfo? cardName = await CheckCardInScreen(windowArea, cachedSplashes, db,
            Properties.COMPARISON_PRECISION);
        if (cardName != null && !cardName.Equals(lastCardSeen))
        {
            Invoke(new Action(() => {
                string logMes = (cardName.Effects.Count == 0 ? "" : Environment.NewLine) + 
                    String.Join(Environment.NewLine, cardName.Effects);
                _logger.WriteToConsole($"{cardName.Name}{logMes}");
            }));
            lastCardSeen = cardName;
        }
    }

    private bool CheckIfInDuelScreen((Point, Point) baseCoords)
    {
        ImageAnalysis ocrRes;

        var points = MasterduelWindow.Window.GetEnemyLP(baseCoords);
        var bm = TakeScreenshotFromArea(points.Item1, points.Item2);
        ocrRes = ocr.ReadImage(bm);

        // Detect LPs in screen
        bool detectedLP = ocrRes.Text.ToLower().Contains("lp");

        bm.Dispose();

        if (!detectedLP) { 
            points = MasterduelWindow.Window.GetYourLP(baseCoords);
            bm = TakeScreenshotFromArea(points.Item1, points.Item2);
            ocrRes = ocr.ReadImage(bm);
            bm.Dispose();
            detectedLP = ocrRes.Text.ToLower().Contains("lp");
        }
           
        return detectedLP;
    }

    private bool CheckIfCardInScreen((Point, Point) baseCoords)
    {
        ImageAnalysis ocrRes;
        string[] validTypes = { "effect", "spell", "trap", "link", "pendulum", "xyz", "synchro", "fusion", "ritual" };
        bool detectedCard = false;
        var points = MasterduelWindow.Window.GetEnemyLP(baseCoords);
        var bm = TakeScreenshotFromArea(points.Item1, points.Item2);

        // Get if card is in screen
        points = MasterduelWindow.Window.GetCardTypeCoords(baseCoords);
        bm = TakeScreenshotFromArea(points.Item1, points.Item2);

        ocrRes = ocr.ReadImage(bm);

        detectedCard = validTypes.Any((x) => ocrRes.Text.ToLower().Contains(x));

        bm.Dispose();
        return detectedCard;
    }

    private async Task<CardInfo?> CheckCardInScreen((Point, Point) baseCoords, MemCache splashCache, LocalDB db, float precision)
    {
        (Point, Point) area;
        Bitmap bm;

        // Check if in duel screen
        if (!_skipDuelScreenCheck && !CheckIfInDuelScreen(baseCoords))
        {
            // DEBUG
            Debug.WriteLine("Skipping card analysis");
            return null;
        }

        // Get splash area coords
        area = MasterduelWindow.Window.GetCardSplashCoords(baseCoords);

        // Take screenshot of area
        bm = TakeScreenshotFromArea(area.Item1, area.Item2);
        var size = db.SplashSize;
        var hash = new ImageHash(bm, size);
        bm.Dispose();
        CardInfo? card = null;

        // See if it's cached in memory
        if (_memCaching && splashCache.CheckInCache(hash))
        {
            card = splashCache.LastLookup;

            if (card != null)
            {
                Debug.WriteLine("Got card cached from Memory");
                return card;
            }
            // TODO: Reevaluate when card is yellow text card
        }

        if (_dbCaching)
        {
            // See if it's in local db
            card = db.GetCardBySplash(hash, precision);

            if (card != null)
            {
                Debug.WriteLine("Got card cached Local DB");
                if (_memCaching)
                {
                    splashCache.AddToCache(hash, card);
                }
                return card;
            }
        }

        // Ultimately, Fecth the API (TODO) 
        if (_skipCardInScreenCheck || CheckIfCardInScreen(baseCoords))
        {
            card = await FecthAPI(baseCoords, hash);
            if (card == null) return null;
            if (card.CardNameIsChanged)
            {
                if (_memCaching)
                {
                    splashCache.AddToCache(hash, card);
                }
                return card;
            }
        }

        if (card != null)
        {
            // DEBUG
            Debug.WriteLine("Caching card into local DB: " + card.Name);
            card.SetSplash(hash);
            db.AddCard(card);
        }
        if (_memCaching)
        {
            // DEBUG
            Debug.WriteLine("Caching card into Memory");
            splashCache.AddToCache(hash, card);
        }

        return card;
    }

    private async Task<CardInfo?> FecthAPI((Point, Point) baseCoords, ImageHash hash)
    {
        (Point, Point) area;
        Bitmap bm = new Bitmap(Properties.SPLASH_SIZE, Properties.SPLASH_SIZE);
        CardInfo? card = null;
            

        area = MasterduelWindow.Window.GetCardTitleCoords(baseCoords);
        bm = TakeScreenshotFromArea(area);
            
        // Check is the card is still the same
        area = MasterduelWindow.Window.GetCardSplashCoords(baseCoords);
        if (!CheckSplashCardTextValidity(area, hash))
        {
            bm.Dispose();
            return null;
        }
        try
        {
            card = await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Light);
            card ??= await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Moderate);
            card ??= await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Aggresive);
        }
        catch (CardNameIsChangedException)
        {
            area = MasterduelWindow.Window.GetCardDescCoords(baseCoords);
            bm = TakeScreenshotFromArea(area);
            card = await CheckCardDescription(bm);
            if (card == null)
            {
                card = new()
                {
                    CardNameIsChanged = true
                };
            }
            Debug.Write("This card has it's name changed! Therefore ethe analysis may not work.");
            return card;
        }

        if (card == null) return null;
        bm.Dispose();
            
        return card;
    }

    private static bool CheckSplashCardTextValidity((Point, Point) area, ImageHash hash)
    {
        Bitmap bm2 = TakeScreenshotFromArea(area.Item1, area.Item2);
        if (!hash.Equals(new ImageHash(bm2, hash.Resolution)))
        {
            return false;
        }
        bm2.Dispose();
        return true;
    }

    private async Task<CardInfo?> CheckCardName(Bitmap bm, 
        TextProcessing.CardText.Trim_aggressiveness aggressiveness)
    {
        // Check if the card name isn't yellow
        if (GetTextColor(bm) == TextColorE.Yellow)
        {
            throw new CardNameIsChangedException("The card name is not it's original");
        }
        ContrastWhitePixels(ref bm, 0.7f, true);

        // Run OCR on image
        ImageAnalysis result = ocr.ReadImage(bm);

        var reformattedCardName = TextProcessing.CardText.TrimCardName(result.Text, aggressiveness);
        // DEBUG
        Debug.WriteLine("Querying API with card name: " + reformattedCardName);
        if (reformattedCardName == "") return null;

        // Fetch the API
        try
        {
            List<CardInfo> apiRes;
                
            apiRes = await CardsAPI.TryGetCardNameAsync(reformattedCardName);
                
            // Check if there aren't many possible cards for the query
            if (!CheckCardAmount(apiRes))
            {
                // INFO
                Debug.WriteLine($"Too many possible cards in the query \"{reformattedCardName}\"");
                return null;
            }

            CardInfo res = TextProcessing.CardText.GetDescFeatures(apiRes.First());
            return res;
        }
        catch (HttpRequestException excp)
        {
            Debug.WriteLine(excp.Message);
        }
        catch (NoCardsFoundException excp)
        {
            Debug.WriteLine(excp.Message);
        }

        return null;
    }

    private async Task<CardInfo?> CheckCardDescription(Bitmap bm)
    {
        ContrastWhitePixels(ref bm, 0.6f, true);
            
        // Run OCR on image
        ImageAnalysis result = ocr.ReadImage(bm, true);
        result.Text = result.Text.Replace('\n', ' ');

        // Fetch the API
        try
        {
            List<CardInfo> apiRes;
            apiRes = await CardsAPI.GetCardDescAsync(result.Text);

            if (!CheckCardAmount(apiRes, 1))
            {
                Debug.WriteLine($"Couldn't get card from description");
                return null;
            }

            CardInfo res = TextProcessing.CardText.GetDescFeatures(apiRes.First());
            return res;
        }
        catch (HttpRequestException excp)
        {
            Debug.WriteLine(excp.Message);
        }
        catch (NoCardsFoundException excp)
        {
            Debug.WriteLine(excp.Message);
        }

        return null;
    }

    private bool CheckCardAmount(List<CardInfo> apiRes, int maxAmount = 0)
    {
        if (maxAmount <= 0) maxAmount = Properties.MAX_POSSIBLE_CARDS_FROM_API;
            
        if (apiRes.Count > maxAmount) return false;
        return true;
    }

    private void StopLoop(object sender, EventArgs e)
    {
        startButton.Enabled = true;
        stopButton.Enabled = false;

    }

    private void LogConsoleCheckBox_changed(object sender, EventArgs e)
    {
        int height_without_console = 150;
        int height_with_console = 300;
        int width = 400;
            
        if (logConsoleCheckBox.Checked)
        {
            consoleLog.Show();
            _logger.ConsoleLog ??= consoleLog;
            this.MinimumSize = new(width, height_with_console);
            this.Size = new(this.Size.Width, height_with_console);
        }
        else
        {
            consoleLog.Hide();
            this.MinimumSize = new(width, height_without_console);
            this.Size = new(this.Size.Width, height_without_console);
        }
    }

    private void consoleLog_TextChanged(object sender, EventArgs e)
    {
        consoleLog.Location = new(12, 106);
    }
}
