using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
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
                ImagePath.Text = open.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        { 

        }

        private void convert_to_text_click(object sender, EventArgs e) {
        
            var handler = new Windows.Handler();
            var wl = handler.GetWindowPoints(Masterduel.WINDOW_NAME);
            var newPoints = Masterduel.Window.GetCardTitleCoords(wl);
            // 16 * 39 offset for borders // <- 8     8 ->
            Bitmap bm = ScreenProcessing.TakeScreenshotFromArea(newPoints.Item1, newPoints.Item2);

            pictureBox1.Image = bm;

            OCR ocr = new();
            var Result = ocr.ReadImage(bm);
            endText.Text = Result.Text;
        }
    }
}