using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using TLDROverlay.Caching;
using TLDROverlay.Config;
using TLDROverlay.Masterduel;
using TLDROverlay.Ocr;
using TLDROverlay.WindowHandler;
using TLDROverlay.WindowHandler.Masterduel;
using TLDROverlay.WindowHandler.Masterduel.Windows;
using TLDROverlay.WindowHandler.Windows;
using static TLDROverlay.Screen.ImageProcessing;
using static TLDROverlay.WindowHandler.IWindowHandler;

namespace TLDROverlay.Engine
{
    public class MasterduelEngine : Engine
    {
        private readonly ConfigLoader _config = ConfigLoader.Instance;
        private CardInfo? lastCardSeen = null;
        private readonly OCR ocr = new();
        private bool _dbCaching = true;
        private bool _memCaching = true;
        private readonly AbstractMasterduelWindow MasterduelWindow = new MasterduelWindow();
        private MemCache MemoryCache;
        private LocalDB DBCache;
        private IWindowHandler? handler;
        
        // debugging attributes
        private bool _skipDuelScreenCheck = false;
        private bool _skipCardInScreenCheck = false;

        // Public attributes
        public bool IsRunning { get; set; } = false;
        public float ComparisonsPrecision { get; }


        public MasterduelEngine(float comparisonsPrecision)
        {
            MemoryCache = new(_config.GetIntProperty(ConfigMappings.MAX_PIXELS_DIFF),
                _config.GetIntProperty(ConfigMappings.SPLASH_SIZE));
            DBCache = new();
            DBCache.Initialize();
            ComparisonsPrecision = comparisonsPrecision;
            MaxCardResultThreshold = _config.GetIntProperty(ConfigMappings.MAX_POSSIBLE_CARDS_FROM_API);
        }

        // Public running methods
        override public void StartLoop()
        {
            try
            {
                handler = new Handler(MasterduelWindow.WindowName);
            }
            catch (NoWindowFoundException)
            {
                throw;
            }

            IsRunning = true;
            // Get window coordinates
            (Point, Point) windowArea;
            try
            {
                windowArea = handler.GetWindowPoints();
                MasterduelWindow.WindowArea = windowArea;
            }
            catch (Exception)
            {
                return;
            }

            new Thread(async () =>
            {
                while (IsRunning)
                {
                    if (handler.IsWindowCurrentlySelected())
                    {
                        await Run();
                    }
                    Thread.Sleep(300);
                }
            }).Start();
        }

        override public void StopLoop()
        {
            IsRunning = false;
        }

        private async Task Run()
        {
            var result = await CheckCardInScreen();
            CardInfo? card = result.Item2;

            if (result.Item1.Equals(CardSearchResullt.NO_MATCH))
            {
                return;
            }

            if (card != null && !card.Equals(lastCardSeen))
            {
                string logMes = (card.Effects.Count == 0 ? "" : Environment.NewLine) + String.Join(Environment.NewLine, card.Effects);
                _logger.Log($"{card.Name}{logMes}", LogLevel.Information);
                lastCardSeen = card;
            }
        }

        public void SetMemoryCaching(bool memCaching)
        {
            _memCaching = memCaching;
        }

        public void SetDBCaching(bool dbCaching)
        {
            _dbCaching = dbCaching;
        }

        public void ClearMemoryCaching()
        {
            MemoryCache.ClearCache();
        }

        public void ClearDBCaching()
        {
            DBCache.ClearDataBase();
        }

        // Internal methods

        public bool CheckIfInDuelScreen()
        {
            ImageAnalysis ocrRes;
            
            var points = MasterduelWindow.EnemyLPCoordinates;
            var bm = TakeScreenshotFromArea(points.Item1, points.Item2);
            ocrRes = ocr.ReadImage(bm);

            // Detect LPs in screen
            bool detectedLP = ocrRes.Text.ToLower().Contains("lp");

            bm.Dispose();

            if (!detectedLP)
            {
                points = MasterduelWindow.YourLPCoordinates;
                bm = TakeScreenshotFromArea(points.Item1, points.Item2);
                ocrRes = ocr.ReadImage(bm);
                bm.Dispose();
                detectedLP = ocrRes.Text.ToLower().Contains("lp");
            }

            return detectedLP;
        }

        public bool CheckIfCardInScreen()
        {
            ImageAnalysis ocrRes;
            string[] validTypes = { "effect", "spell", "trap", "link", "pendulum", "xyz", "synchro", "fusion", "ritual" };
            //  Maybe add the card type to CardInfo and the API call?
            /*
                Type accepts 'Skill Card', 'Spell Card', 'Trap Card', 'Normal Monster', 'Normal Tuner Monster', 
                'Effect Monster', 'Tuner Monster', 'Flip Monster', 'Flip Effect Monster', 'Flip Tuner Effect Monster', 
                'Spirit Monster', 'Union Effect Monster', 'Gemini Monster', 'Pendulum Effect Monster', 'Pendulum Normal Monster', 
                'Pendulum Tuner Effect Monster', 'Ritual Monster', 'Ritual Effect Monster', 'Toon Monster', 'Fusion Monster', 
                'Synchro Monster', 'Synchro Tuner Monster', 'Synchro Pendulum Effect Monster', 'XYZ Monster', 
                'XYZ Pendulum Effect Monster', 'Link Monster', 'Pendulum Flip Effect Monster', 
                'Pendulum Effect Fusion Monster' or 'Token' and is not case sensitive."
            */
            bool detectedCard = false;

            // Get if card is in screen
            var points = MasterduelWindow.CardTypeCoordinates;
            var bm = TakeScreenshotFromArea(points.Item1, points.Item2);

            ocrRes = ocr.ReadImage(bm);

            detectedCard = validTypes.Any((x) => ocrRes.Text.ToLower().Contains(x));

            bm.Dispose();
            return detectedCard;
        }

        public enum CardSearchResullt
        {   
            MATCH,
            NO_MATCH
        }

        public async Task<(CardSearchResullt, CardInfo?)> CheckCardInScreen()
        {
            (Point, Point) area;
            Bitmap bm;

            // Check if in duel screen
            if (!_skipDuelScreenCheck && !CheckIfInDuelScreen())
            {
                // DEBUG
                _logger.Log("Skipping card analysis", LogLevel.Trace);
                return (CardSearchResullt.NO_MATCH, null);
            }

            // Get splash area coords
            area = MasterduelWindow.CardSplashCoordinates;

            // Take screenshot of area
            bm = TakeScreenshotFromArea(area.Item1, area.Item2);
            var size = DBCache.SplashSize;
            var hash = new ImageHash(bm, size);
            bm.Dispose();
            CardInfo? card;

            // See if it's cached in memory
            if (_memCaching && MemoryCache.CheckInCache(hash))
            {
                card = MemoryCache.LastLookup;

                if (card != null)
                {
                    _logger.Log("Got card cached from Memory", LogLevel.Trace);
                    return (CardSearchResullt.MATCH, card);
                }
                else
                {
                    return (CardSearchResullt.MATCH, null);
                }
            }

            if (_dbCaching)
            {
                // See if it's in local db
                card = DBCache.GetCardBySplash(hash, ComparisonsPrecision);

                if (card != null)
                {
                    _logger.Log("Got card cached Local DB", LogLevel.Trace);
                    if (_memCaching)
                    {
                        MemoryCache.AddToCache(hash, card);
                    }
                    return (CardSearchResullt.MATCH, card);
                }
            }

            // Ultimately, Fecth the API
            if (_skipCardInScreenCheck || !CheckIfCardInScreen())
            {
                return (CardSearchResullt.NO_MATCH, null);
            }
            
            card = await FecthAPI(MasterduelWindow.CardTitleCoordinates, MasterduelWindow.CardSplashCoordinates, hash, MasterduelWindow.CardDescCoordinates);
            _logger.Log(card?.ToString() ?? string.Empty);
            if (card == null) return (CardSearchResullt.NO_MATCH, null);
            if (card.CardNameIsChanged)
            {
                if (_memCaching)
                {
                    MemoryCache.AddToCache(hash, card);
                }
                return (CardSearchResullt.MATCH, card);
                
            }

            if (_dbCaching)
            {
                // DEBUG
                _logger.Log("Caching card into local DB: " + card.Name, LogLevel.Debug);
                card.SetSplash(hash);
                DBCache.AddCard(card);
            }
            if (_memCaching)
            {
                // DEBUG
                _logger.Log("Caching card into Memory", LogLevel.Debug);
                MemoryCache.AddToCache(hash, card);
            }

            return (CardSearchResullt.MATCH, card);
        }
    }
}
