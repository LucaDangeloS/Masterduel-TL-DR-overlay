using Patagames.Ocr;
using Patagames.Ocr.Enums;

namespace Masterduel_TLDR_overlay.Ocr
{
    internal class OCR
    {
        private OcrApi api;

        // Public methods
        public OCR()
        {
            api = OcrApi.Create();
            api.Init(Languages.English);
        }
        /// <summary>
        /// Recognizes text from image using the Tesseract OCR engine.
        /// </summary>
        /// <param name="bm">The <see cref="Bitmap"/> object from which the OCR algorith will read.</param>
        /// <returns>Returns an <see cref="ImageAnalysis"/> object, containing among other things the text recognized from the image. 
        /// If no text was recognized the <see cref="ImageAnalysis.Text"/> field will countain an empty string.</returns>
        public ImageAnalysis ReadImage(Bitmap bm)
        {
            string plainText;
            ImageAnalysis ret = new ImageAnalysis();

            try
            {
              plainText = api.GetTextFromImage(bm);
                ret.Text = plainText;
            } catch (Exception) { }

            return ret;
        }
    }

    public struct ImageAnalysis
    {
        public string Text;

        public ImageAnalysis()
        {
            Text = "";
        }
    }
}
