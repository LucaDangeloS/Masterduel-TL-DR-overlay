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

namespace Masterduel_TLDR_overlay
{
    public partial class Form : System.Windows.Forms.Form
    {
        private PropertiesC Properties = PropertiesLoader.Instance.Properties;

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

            try
            {
                handler = new Handler(MasterduelWindow.WINDOW_NAME);
            }
            catch (NoWindowFoundException)
            {
                Debug.WriteLine("Could not find the window. Please make sure it is open and try again.");
                return;
            }

            MemCache cachedSplashes = new(Properties.MAX_PIXELS_DIFF);
            LocalDB db = new();
            db.Initialize();

            new Thread(() =>
            {
                DateTime start = DateTime.UtcNow;

                while (stopButton.Enabled)
                {
                    // Get current window

                    if (handler.IsWindowCurrentlySelected())
                    {
                        //if (handler.GetLeftMousePressed())
                        //{
                        DoThings(handler, cachedSplashes, db);
                        //}
                    }
                    Thread.Sleep(500);
                }
            }).Start();
        }

        private async void DoThings(WindowHandlerInterface handler, MemCache cachedSplashes, LocalDB db)
        {
            (Point, Point) windowArea;
            try
            {
                windowArea = handler.GetWindowPoints();
            }
            catch (Exception)
            {
                Invoke(new Action(() => {
                    endText.Text = "No window was found";
                }));
                return;
            }
            CardInfo? cardName = await CheckCardInScreen(windowArea, cachedSplashes, db,
                Properties.COMPARISON_PRECISION);
            Debug.WriteLine(cardName?.ToString());
        }

        private async Task<CardInfo?> CheckCardInScreen((Point, Point) baseCoords, MemCache splashCache, LocalDB db, float precision)
        {
            (Point, Point) area;
            Bitmap bm;

            // Get splash area coords
            area = MasterduelWindow.Window.GetCardSplashCoords(baseCoords);

            // Take screenshot of area
            bm = ImageProcessing.TakeScreenshotFromArea(area.Item1, area.Item2);
            Invoke(new Action(() => {
                // Debugging purposes
                pictureBox1.Image = (Image)bm.Clone();
            }));
            var size = db.SplashSize;
            var hash = new ImageHash(bm, size);

            // See if it's cached in memory
            if (splashCache.CheckInCache(hash, precision))
            {
                Debug.WriteLine("Got from the Memory Cache!");
                return splashCache.LastLookup;
            }

            CardInfo? card;

            // See if it's in local db
            card = db.GetCardBySplash(hash, precision);

            bm.Dispose();

            if (card != null)
            {
                Debug.WriteLine("Got from the Local DB!");
                splashCache.AddToCache(hash, card);
                return card;
            }

            // Ultimately, Fecth the API

            // Check if card image is still the same
            // TODO: First Take screenshot of text, then from the splash, and then compare the splashes.
            // If the splashes don't match, drop. If the text isn't recognized, repeat the last 2 steps X times.
            // If it fails X or more times, add it as a semi-permanent entry on cache indicating it not to do further checking.
            if (!CheckSplashCardTextValidity(area, hash, precision)) return null;

            // Fetch the API
            area = MasterduelWindow.Window.GetCardTitleCoords(baseCoords);
            bm = TakeScreenshotFromArea(area);
            Invoke(new Action(() => {
                pictureBox1.Image = (Image)bm.Clone();
            }));
            card = await CheckText(bm);

            bm.Dispose();

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

        private static async Task<CardInfo?> CheckText(Bitmap bm)
        {
            // Run OCR on image
            OCR ocr = new();
            var Result = ocr.ReadImage(bm);
            var reformattedCardName = TextProcessing.CardText.TrimCardName(Result.Text, TextProcessing.CardText.Trim_aggressiveness.Light);

            if (reformattedCardName == "") return null;

            try
            {
                List<CardInfo> apiRes;
                if (reformattedCardName.Length > 10)
                {
                    apiRes = await CardsAPI.GetCardByNameAsync(reformattedCardName);
                }
                else
                {
                    apiRes = await CardsAPI.GetCardByExactNameAsync(reformattedCardName);
                }

                // TODO: Change just retrieving the first card in the api response
                if (apiRes == null) return null;
                CardInfo res = TextProcessing.CardText.GetDescFeatures(apiRes[0]);
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