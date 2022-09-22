using Masterduel_TLDR_overlay.Ocr;
using Masterduel_TLDR_overlay.Screen;
using System.Diagnostics;
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
            var wl = handler.GetWindowPoints("masterduel");
            //var newPoints = (new Point(wl.Item1.X + Masterduel.X_INIT_OFFSET, wl.Item1.Y + Masterduel.Y_INIT_OFFSET),
            //                 new Point(wl.Item1.X + Masterduel.X_END_OFFSET, wl.Item1.Y + Masterduel.Y_END_OFFSET));
            Size size = new(wl.Item2.X - wl.Item1.X, wl.Item2.Y - wl.Item1.Y);
            var newPoints = (new Point((int) ((float) size.Width * Masterduel.X_INIT_SCALE), (int) ((float) size.Height * Masterduel.Y_INIT_SCALE)),
                             new Point((int) ((float) size.Width * Masterduel.X_END_SCALE), (int) ((float)size.Height * Masterduel.Y_END_SCALE)));
            //Debug.WriteLine("X: " + (float)newPoints.Item1.X / (float)(wl.Item2.X - wl.Item1.X)  + "  Y: " + (float)newPoints.Item1.Y / (float)(wl.Item2.Y - wl.Item1.Y));
            //Debug.WriteLine("X: " + (float)newPoints.Item2.X / (float)(wl.Item2.X - wl.Item1.X)  + "  Y: " + (float)newPoints.Item2.Y / (float)(wl.Item2.Y - wl.Item1.Y));
            Debug.WriteLine("X: " + newPoints.Item1.X + "  Y: " + newPoints.Item1.Y);
            Bitmap bm = ScreenProcessing.TakeScreenshotFromArea(newPoints.Item1, newPoints.Item2);

            pictureBox1.Image = bm;

            OCR ocr = new();
            var Result = ocr.ReadImage(bm);
            endText.Text = Result.Text;
        }
    }
}