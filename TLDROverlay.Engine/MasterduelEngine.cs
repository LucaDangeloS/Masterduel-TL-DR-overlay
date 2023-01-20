using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLDROverlay.Caching;
using TLDROverlay.Config;
using TLDROverlay.Exceptions;
using TLDROverlay.Masterduel;
using TLDROverlay.Ocr;
using TLDROverlay.WindowHandler.Masterduel.Windows;
using TLDROverlay.WindowHandler.Windows;
using static TLDROverlay.Screen.ImageProcessing;

namespace TLDROverlay.Engine
{
    public class MasterduelEngine : Engine
    {
        private readonly ConfigLoader _config = ConfigLoader.Instance;
        private CardInfo? lastCardSeen = null;
        private readonly OCR ocr = new();
        private bool _dbCaching = true;
        private bool _memCaching = true;

        private MemCache MemoryCache;
        private LocalDB DBCache;

        private static object SyncLock = new object();
        public bool IsRunning { get; set; } = false;
        public float ComparisonsPrecision { get; }

        public MasterduelEngine(float comparisonsPrecision)
        {
            MemoryCache = new(_config.GetIntProperty(ConfigMappings.MAX_PIXELS_DIFF),
                _config.GetIntProperty(ConfigMappings.SPLASH_SIZE));
            DBCache = new();
            DBCache.Initialize();
            ComparisonsPrecision = comparisonsPrecision;
        }

        public bool CheckIfInDuelScreen((Point, Point) EnemyLp, (Point, Point) YourLp)
        {
            ImageAnalysis ocrRes;
            
            var points = EnemyLp;
            var bm = TakeScreenshotFromArea(points.Item1, points.Item2);
            ocrRes = ocr.ReadImage(bm);

            // Detect LPs in screen
            bool detectedLP = ocrRes.Text.ToLower().Contains("lp");

            bm.Dispose();

            if (!detectedLP)
            {
                points = YourLp;
                bm = TakeScreenshotFromArea(points.Item1, points.Item2);
                ocrRes = ocr.ReadImage(bm);
                bm.Dispose();
                detectedLP = ocrRes.Text.ToLower().Contains("lp");
            }

            return detectedLP;
        }

        public bool CheckIfCardInScreen((Point, Point) cardTypeRect)
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
            var points = cardTypeRect;
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

        public async Task<(CardSearchResullt, CardInfo?)> CheckCardInScreen((Point, Point) cardSplashRect,
            (Point, Point) cardTitleRect, (Point, Point) cardDescRect)
        {
            (Point, Point) area;
            Bitmap bm;

            // Get splash area coords
            area = cardSplashRect;

            // Take screenshot of area
            bm = TakeScreenshotFromArea(area.Item1, area.Item2);
            var size = DBCache.SplashSize;
            var hash = new ImageHash(bm, size);
            bm.Dispose();
            CardInfo? card = null;

            // See if it's cached in memory
            if (_memCaching && MemoryCache.CheckInCache(hash))
            {
                card = MemoryCache.LastLookup;

                if (card != null)
                {
                    Debug.WriteLine("Got card cached from Memory");
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
                    Debug.WriteLine("Got card cached Local DB");
                    if (_memCaching)
                    {
                        MemoryCache.AddToCache(hash, card);
                    }
                    return (CardSearchResullt.MATCH, card);
                }
            }

            // Ultimately, Fecth the API
            card = await FecthAPI(cardTitleRect, cardSplashRect, hash, cardDescRect);
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
                Debug.WriteLine("Caching card into local DB: " + card.Name);
                card.SetSplash(hash);
                DBCache.AddCard(card);
            }
            if (_memCaching)
            {
                // DEBUG
                Debug.WriteLine("Caching card into Memory");
                MemoryCache.AddToCache(hash, card);
            }

            return (CardSearchResullt.MATCH, card);
        }
    }
}
