using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        { 

        }

        private async void convert_to_text_clickAsync(object sender, EventArgs e) {

            var handler = new Windows.Handler();
            var wl = handler.GetWindowPoints(Masterduel.WINDOW_NAME);
            var newPoints = Masterduel.Window.GetCardTitleCoords(wl);
            // 16 * 39 offset for borders // <- 8     8 ->
            Bitmap bm = ScreenProcessing.TakeScreenshotFromArea(newPoints.Item1, newPoints.Item2);

            pictureBox1.Image = bm;

            OCR ocr = new();
            var Result = ocr.ReadImage(bm);
            var reformattedCardName = TextProcessing.CardText.TrimCardName(Result.Text, TextProcessing.CardText.Trim_aggressiveness.Aggresive);
            endText.Text = "Original: " + Result.Text + "\r\n" + reformattedCardName;
            try
            {
                List<CardInfo> apiRes = await CardsAPI.GetCardByNameAsync(reformattedCardName);
                endText.Text = endText.Text + "\r\n\r\n" + string.Join("\r\n\r\n", apiRes);
            } catch (HttpRequestException excp)
            {
                Debug.WriteLine(excp.Message);
            } catch (NoCardsFoundException excp)
            {
                Debug.WriteLine(excp.Message);
            }

        }
    }
}