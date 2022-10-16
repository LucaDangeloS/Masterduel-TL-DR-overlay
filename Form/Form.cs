using Masterduel_TLDR_overlay.Masterduel;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
using System.Diagnostics;
using Masterduel_TLDR_overlay.WindowHandlers;
using static Masterduel_TLDR_overlay.WindowHandlers.WindowHandlerInterface;
using Masterduel_TLDR_overlay.Caching;
using Masterduel_TLDR_overlay.Windows;
using static Masterduel_TLDR_overlay.Screen.ImageProcessing;
using static PropertiesLoader;
using Masterduel_TLDR_overlay.Overlay;
using static SQLite.SQLite3;

namespace Masterduel_TLDR_overlay
{
    public partial class Form : System.Windows.Forms.Form
    {
        private PropertiesC Properties = PropertiesLoader.Instance.Properties;
        OCR ocr = new();
        
        public Form()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void upload_image_click(object sender, EventArgs e)
        {
            OpenFileDialog open = new();
            // image filters  
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // display image in picture box  
                pictureBox1.Image = new Bitmap(open.FileName);
                // image file path  
            }
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
                Debug.WriteLine("Could not find the window. Please make sure it is open and try again.");
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
            Debug.WriteLine(cardName?.ToString());
        }

        private bool CheckIfInDuelScreen((Point, Point) baseCoords)
        {
            ImageAnalysis ocrRes;

            var points = MasterduelWindow.Window.GetEnemyLP(baseCoords);
            var bm = TakeScreenshotFromArea(points.Item1, points.Item2);
            bool detectedLP = false;
            ocrRes = ocr.ReadImage(bm);
            
            // Detect LPs in screen
            detectedLP = ocrRes.Text.ToLower().Contains("lp");

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
            
            // Get contrasted image
            if (!detectedCard)
            {
                Bitmap contrastedBm = (Bitmap)bm.Clone();
                ContrastWhitePixels(contrastedBm);
                ocrRes = ocr.ReadImage(bm);
                detectedCard = validTypes.Any((x) => ocrRes.Text.ToLower().Contains(x));
                contrastedBm.Dispose();
            }
            Invoke(new Action(() =>
            {
                // Debugging purposes
                endText.Text += ocrRes.Text.ToLower();
            }));
            

            // Last effort for Trap cards...
            if (!detectedCard)
            {
                var t = ocrRes.Text.ToLower().Trim();
                if (t == "i" || t == "e") detectedCard = true;
            }

            bm.Dispose();
            return detectedCard;
        }

        private async Task<CardInfo?> CheckCardInScreen((Point, Point) baseCoords, MemCache splashCache, LocalDB db, float precision)
        {
            (Point, Point) area;
            Bitmap bm;

            // Check if in duel screen
            if (!CheckIfInDuelScreen(baseCoords))
            {
                Debug.WriteLine("Skipping card analysis");
                return null;
            }

            // Get splash area coords
            area = MasterduelWindow.Window.GetCardSplashCoords(baseCoords);

            // Take screenshot of area
            bm = TakeScreenshotFromArea(area.Item1, area.Item2);
            var size = db.SplashSize;
            var hash = new ImageHash(bm, size);
            CardInfo? card = null;

            // See if it's cached in memory
            if (splashCache.CheckInCache(hash, precision))
            {
                Debug.WriteLine("Got from the Memory Cache!");
                card = splashCache.LastLookup;
                if (card != null)
                    Invoke(new Action(() => {
                        endText.Text = String.Join("\r\n", card.Effects);
                    }));
                return splashCache.LastLookup;
            }


            // See if it's in local db
            card = db.GetCardBySplash(hash, precision);

            bm.Dispose();

            if (card != null)
            {
                Debug.WriteLine("Got from the Local DB!");
                splashCache.AddToCache(hash, card);
                if (card != null)
                    Invoke(new Action(() => {
                        endText.Text = String.Join("\r\n", card.Effects);
                    }));
                return card;
            }

            // Ultimately, Fecth the API
            if (CheckIfCardInScreen(baseCoords))
            {
                card = await FecthAPI(baseCoords, hash);
                if (card == null) return null;
            }

            if (card != null)
            {
                //Cache the card in DB
                Debug.WriteLine("Caching card into local DB: " + card.Name);
                card.SetSplash(hash);
                db.AddCard(card);
            }
            //Cache in local memory
            Debug.WriteLine("Caching splash into memory");
            splashCache.AddToCache(hash, card);

            if (card !=  null)
                Invoke(new Action(() => {
                    endText.Text = String.Join("\r\n", card.Effects);
                }));

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
            if (!CheckSplashCardTextValidity(area, hash, Properties.COMPARISON_PRECISION))
            {
                bm.Dispose();
                return null;
            }

            card = await CheckText(bm, TextProcessing.CardText.Trim_aggressiveness.Light);
            if (card == null) return null;

            Invoke(new Action(() =>
            {
                // Debugging purposes
                pictureBox1.Image = (Image)bm.Clone();
            }));
            bm.Dispose();
            
            return card;
        }

        private static bool CheckSplashCardTextValidity((Point, Point) area, ImageHash hash, float precision)
        {
            Bitmap bm2 = TakeScreenshotFromArea(area.Item1, area.Item2);
            if (hash.CompareTo(new ImageHash(bm2, hash.Resolution)) < precision)
            {
                return false;
            }
            bm2.Dispose();
            return true;
        }

        private async Task<CardInfo?> CheckText(Bitmap bm, 
            TextProcessing.CardText.Trim_aggressiveness aggressiveness)
        {
            // Run OCR on image
            
            var result = ocr.ReadImage(bm);
            Invoke(new Action(() =>
            {
                // Debugging purposes
                endText.Text = result.Text;
            }));

            var reformattedCardName = TextProcessing.CardText.TrimCardName(result.Text, aggressiveness);

            if (reformattedCardName == "") return null;

            try
            {
                List<CardInfo> apiRes;
                if (aggressiveness > TextProcessing.CardText.Trim_aggressiveness.Light)
                {
                    apiRes = await CardsAPI.GetCardByNameAsync(reformattedCardName);
                }
                else
                {
                    apiRes = await CardsAPI.TryGetCardNameAsync(reformattedCardName);
                }

                // TODO: Change just retrieving the first card in the api response
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
            finally
            {
                Debug.WriteLine(reformattedCardName);
            }

            return null;
        }

        private void StopLoop(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            stopButton.Enabled = false;

        }
    }
}