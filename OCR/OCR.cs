using Patagames.Ocr;
using Patagames.Ocr.Enums;

namespace Masterduel_TLDR_overlay.Ocr
{
    internal class OCR
    {
        private OcrApi api;

        public OCR()
        {
            api = OcrApi.Create();
            api.Init(Languages.English);
        }
        public ImageAnalysis ReadImage(Bitmap bm)
        {
            string plainText;
            ImageAnalysis ret = new ImageAnalysis();

            try
            {
              plainText = api.GetTextFromImage(bm);
                ret.Text = plainText;
            } catch (Exception)
            {
            }

            return ret;
        }
    }

    public struct ImageAnalysis
    {
        public string Text;
    }
}
