using Masterduel_TLDR_overlay.Masterduel;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
using System.Diagnostics;
using Masterduel_TLDR_overlay.WindowHandlers;
using static Masterduel_TLDR_overlay.Masterduel.CardInfo;
using static Masterduel_TLDR_overlay.WindowHandlers.WindowHandlerInterface;

namespace Masterduel_TLDR_overlay
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void upload_image_click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            // image filters  
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // display image in picture box  
                pictureBox1.Image = new Bitmap(open.FileName);
                // image file path  
            }
        }


        private void StartLoop(object sender, EventArgs e) {
            startButton.Enabled = false;
            stopButton.Enabled = true;

            WindowHandlerInterface handler;
            //bool mouseClicked;
            
            try {
                handler = new Windows.Handler("Master Duel");
            } catch (NoWindowFoundException) {
                Debug.WriteLine("Could not find the window. Please make sure it is open and try again.");
                return;
            }
         
            List<Bitmap> cachedSplashes = new();

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
                        CheckCardInScreen(handler, cachedSplashes);
                        //}
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private async void CheckCardInScreen(WindowHandlerInterface handler, List<Bitmap> cachedSplashes)
        {
            // Get window coordinates
            (Point, Point) wl, newPoints, splash;
            try
            {
                wl = handler.GetWindowPoints(MasterduelWindow.WINDOW_NAME);
                newPoints = MasterduelWindow.Window.GetCardTitleCoords(wl);
                splash = MasterduelWindow.Window.GetCardSplashCoords(wl);
            } catch (Exception) {
                endText.Text = "No window was found";
                return;
            }

            // Take screenshot of area
            Bitmap bm = ScreenProcessing.TakeScreenshotFromArea(newPoints.Item1, newPoints.Item2);
            pictureBox1.Image = bm;
            
            //Caching (not implemented)

            // Run OCR on image
            OCR ocr = new();
            var Result = ocr.ReadImage(bm);
            var reformattedCardName = TextProcessing.CardText.TrimCardName(Result.Text, TextProcessing.CardText.Trim_aggressiveness.Light);

            if (reformattedCardName == "") return;

            try
            {
                Bitmap splashBm = ScreenProcessing.TakeScreenshotFromArea(splash.Item1, splash.Item2);
                if (cachedSplashes.Any((s) => ScreenProcessing.CompareImages(s, splashBm) >= 0.94))
                {
                    Debug.WriteLine("Splash already cached!");
                }
                List<CardInfo> apiRes;
                if (reformattedCardName.Length > 10)
                {
                    apiRes = await CardsAPI.GetCardByNameAsync(reformattedCardName);
                }
                else
                {
                    apiRes = await CardsAPI.GetCardByExactNameAsync(reformattedCardName);
                }

                cachedSplashes.Add(splashBm);

                SummarizedData res = TextProcessing.CardText.GetDescFeatures(apiRes[0]);
                Invoke(new Action(() =>
                {
                    var effectsList = res.GetEffects();
                    
                    if (effectsList != null) {
                        endText.Text = effectsList.Aggregate("", (current, effect) => current + (effect.ToString()));
                    } else {
                        endText.Text = "";
                    }
                }));
            }
            catch (HttpRequestException excp)
            {
                Debug.WriteLine(excp.Message);
            }
            catch (NoCardsFoundException excp)
            {
                Debug.WriteLine(excp.Message);
            }
        }

        private void StopLoop(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            stopButton.Enabled = false;
            
        }
    }
}