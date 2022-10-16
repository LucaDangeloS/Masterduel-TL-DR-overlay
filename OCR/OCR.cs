using Patagames.Ocr;
using Patagames.Ocr.Enums;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

            plainText = api.GetTextFromImage(bm);
            ret.Text = ProcessExclusions(plainText);

            return ret;
        }

        private static string ProcessExclusions(string rawString)
        {
            // Evil Twin
            Regex evilRegex = new Regex("(.*?vil).{ 0,4}?(Twins?)(.*)");
            Regex liveRegex = new Regex("(.*?ive).{ 0,4}?(Twins?)(.*)");
            string ret = rawString;

            if (evilRegex.IsMatch(rawString))
            {
                Match m = evilRegex.Match(rawString);
                Group evilGroup = m.Groups[0];
                Group twinGroup = m.Groups[1];
                Group restGroup = m.Groups[2];

                if (evilGroup.Success)
                {
                    ret = evilGroup.Value + @"★" + twinGroup.Value + restGroup.Value;
                }
            }
            // Live Twin
            else if (liveRegex.IsMatch(rawString))
            {
                Match m = evilRegex.Match(rawString);
                Group evilGroup = m.Groups[0];
                Group twinGroup = m.Groups[1];
                Group restGroup = m.Groups[2];

                if (evilGroup.Success)
                {
                    ret = evilGroup.Value + @"☆" + twinGroup.Value + restGroup.Value;
                }
            }
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
