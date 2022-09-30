using Masterduel_TLDR_overlay.Masterduel;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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


        private async void convert_to_text_clickAsync(object sender, EventArgs e) {

            // top level
            //var handler = new Windows.Handler();
            //var mouseClicked = Windows.Handler.GetLeftMousePressed();
            //Debug.WriteLine(mouseClicked);
            //TextProcessing.CardText.GetDescFeatures(new CardInfo("text", "text"));

            // Get current window
            //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            //static extern IntPtr GetForegroundWindow();
            //Debug.WriteLine(GetForegroundWindow() + "  " + handler.WinHandle);
            
            
            // bottom level
            //var wl = handler.GetWindowPoints(MasterduelWindow.WINDOW_NAME);
            //var newPoints = MasterduelWindow.Window.GetCardTitleCoords(wl);

            //Bitmap bm = ScreenProcessing.TakeScreenshotFromArea(newPoints.Item1, newPoints.Item2);
            //pictureBox1.Image = bm;

            //if (pictureBox1.Image != null)
            //{
            //    try
            //    {
            //        // REPLACE
            //        Bitmap bm2 = new Bitmap(@"temp.jpg");
            //        float comp = ScreenProcessing.CompareImages(bm, bm2);
            //        Debug.WriteLine(comp);
            //        bm2.Dispose();
            //    } catch (Exception excp)
            //    {
            //        Debug.WriteLine(excp);
            //    }
            //}

            //OCR ocr = new();
            //var Result = ocr.ReadImage(bm);
            //var reformattedCardName = TextProcessing.CardText.TrimCardName(Result.Text, TextProcessing.CardText.Trim_aggressiveness.Light);
            //endText.Text = "Original: " + Result.Text + "\r\n" + reformattedCardName;
            //try
            //{
            //    List<CardInfo> apiRes;
            //    if (reformattedCardName.Length > 10)
            //    {
            //        apiRes = await CardsAPI.GetCardByNameAsync(reformattedCardName);
            //    } else
            //    {
            //        apiRes = await CardsAPI.GetCardByExactNameAsync(reformattedCardName);
            //    }
            //    endText.Text = endText.Text + "\r\n\r\n" + string.Join("\r\n\r\n", apiRes);
            //    TextProcessing.CardText.GetDescFeatures(apiRes[0]);
            //} catch (HttpRequestException excp)
            //{
            //    Debug.WriteLine(excp.Message);
            //} catch (NoCardsFoundException excp)
            //{
            //    Debug.WriteLine(excp.Message);
            //}

        }
    }
}