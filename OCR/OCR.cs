using Patagames.Ocr;
using Patagames.Ocr.Enums;

namespace Masterduel_TLDR_overlay.OCR
{
    internal class OCR
    {
        private OcrApi api;

        public OCR()
        {
            api = OcrApi.Create();
            api.Init(Languages.English);
        }
        public ImageAnalysis ReadImage(string imagePath)
        {
            string plainText = api.GetTextFromImage(imagePath);

            ImageAnalysis ret = new ImageAnalysis();
            ret.Text = plainText;

            return ret;
        }
    }

    public struct ImageAnalysis
    {
        public string Text;
    }
}
