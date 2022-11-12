using System.Diagnostics;
using System.Text.RegularExpressions;
using TesseractOCR;

namespace TLDROverlay.Ocr;

internal class OCR
{
    private Engine _engine_5;
    private readonly bool _isDebug = true;

    // Public methods
    public OCR()
    {
        _engine_5 = new Engine(@"./tessdata", "MasterduelEng", TesseractOCR.Enums.EngineMode.Default);
        _engine_5.DefaultPageSegMode = TesseractOCR.Enums.PageSegMode.SingleLine;
    }

    /// <summary>
    /// Recognizes text from image using the Tesseract OCR engine.
    /// </summary>
    /// <param name="bm">The <see cref="Bitmap"/> object from which the OCR algorith will read.</param>
    /// <returns>Returns an <see cref="ImageAnalysis"/> object, containing among other things the text recognized from the image. 
    /// If no text was recognized the <see cref="ImageAnalysis.Text"/> field will countain an empty string.</returns>
    public ImageAnalysis ReadImage(Bitmap bm, bool multiline = false)
    {
        string plainText;

        MemoryStream ms = new ();
        if (_isDebug)
        {
            bm.Save("lastCheck.png");
        }
        bm.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        var img = TesseractOCR.Pix.Image.LoadFromMemory(ms);
        Page processesImg;

        if (!multiline)
        {
            processesImg = _engine_5.Process(img);
        } else
        {
            processesImg = _engine_5.Process(img, TesseractOCR.Enums.PageSegMode.SingleBlock);
        }
        plainText = processesImg.Text;
        processesImg.Dispose();

        return new ImageAnalysis(ProcessExclusions(plainText));
    }

    // Private methods
    private static string ProcessExclusions(string rawString)
    {
        // Evil Twin
        Regex evilRegex = new Regex("(.*?vil).{0,4}?(Twins?)(.*)", RegexOptions.IgnoreCase);
        Regex liveRegex = new Regex("(.*?ive).{0,4}?(Twins?)(.*)", RegexOptions.IgnoreCase);
        string ret = rawString;

        if (evilRegex.IsMatch(rawString))
        {
            Match m = evilRegex.Match(rawString);
            Group evilGroup = m.Groups[1];
            Group twinGroup = m.Groups[2];
            Group restGroup = m.Groups[3];

            if (evilGroup.Success)
            {
                ret = evilGroup.Value + @"★" + twinGroup.Value + restGroup.Value;
            }
        }
        // Live Twin
        else if (liveRegex.IsMatch(rawString))
        {
            Match m = liveRegex.Match(rawString);
            Group liveGroup = m.Groups[1];
            Group twinGroup = m.Groups[2];
            Group restGroup = m.Groups[3];

            if (liveGroup.Success)
            {
                ret = liveGroup.Value + @"☆" + twinGroup.Value + restGroup.Value;
            }
        }

        return ret;
    }

}

public struct ImageAnalysis
{
    public string Text;

    public ImageAnalysis(string text = "")
    {
        Text = text;
    }
}

