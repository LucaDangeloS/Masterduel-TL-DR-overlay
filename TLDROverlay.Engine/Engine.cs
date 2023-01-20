using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TLDROverlay.Screen.ImageProcessing;
using TLDROverlay.Config;
using TLDROverlay.Exceptions;
using TLDROverlay.Masterduel;
using TLDROverlay.WindowHandler.Masterduel.Windows;
using TLDROverlay.Api;
using TLDROverlay.Ocr;
using System.Drawing;

namespace TLDROverlay.Engine
{
    public abstract class Engine
    {
        private readonly OCR ocr = new();
        public int MaxCardResultThreshold;
        public int SplashSizes;

        public virtual bool CheckSplashCardTextValidity((Point, Point) splashRect, ImageHash hash)
        {
            Bitmap bm2 = TakeScreenshotFromArea(splashRect.Item1, splashRect.Item2);
            if (!hash.Equals(new ImageHash(bm2, hash.Resolution)))
            {
                return false;
            }
            bm2.Dispose();
            return true;
        }

        public virtual async Task<CardInfo?> CheckCardName(Bitmap bm,
                         TextProcessing.CardText.Trim_aggressiveness aggressiveness)
        {
            // Check if the card name isn't yellow
            if (GetTextColor(bm) == TextColorE.Yellow)
            {
                throw new CardNameIsChangedException("The card name is not it's original");
            }
            ContrastWhitePixels(ref bm, 0.7f, true);

            // Run OCR on image
            ImageAnalysis result = ocr.ReadImage(bm);

            var reformattedCardName = TextProcessing.CardText.TrimCardName(result.Text, aggressiveness);
            // DEBUG
            Debug.WriteLine($"Querying API with card name: '{reformattedCardName}' with {aggressiveness}");
            if (reformattedCardName == "") return null;

            // Fetch the API
            try
            {
                List<CardInfo> apiRes;

                apiRes = await CardsAPI.TryGetCardNameAsync(reformattedCardName);

                // Check if there aren't many possible cards for the query
                if (!CheckCardAmount(apiRes, MaxCardResultThreshold))
                {
                    // INFO
                    Debug.WriteLine($"Too many possible cards in the query \"{reformattedCardName}\"");
                    return null;
                }

                CardInfo res = TextProcessing.CardText.GetDescFeatures(apiRes.First());
                return res;
            }
            catch (HttpRequestException excp)
            {
                Debug.WriteLine(excp.Message);
            }
            catch (NoCardsFoundException excp)
            {
                Debug.WriteLine(excp.Message);
            }

            return null;
        }

        public virtual async Task<CardInfo?> CheckCardDescription(Bitmap bm)
        {
            ContrastWhitePixels(ref bm, 0.6f, true);

            // Run OCR on image
            ImageAnalysis result = ocr.ReadImage(bm, true);
            result.Text = result.Text.Replace('\n', ' ');

            // Fetch the API
            try
            {
                List<CardInfo> apiRes;
                apiRes = await CardsAPI.GetCardDescAsync(result.Text);

                if (!CheckCardAmount(apiRes, 1))
                {
                    Debug.WriteLine($"Couldn't get card from description");
                    return null;
                }

                CardInfo res = TextProcessing.CardText.GetDescFeatures(apiRes.First());
                return res;
            }
            catch (HttpRequestException excp)
            {
                Debug.WriteLine(excp.Message);
            }
            catch (NoCardsFoundException excp)
            {
                Debug.WriteLine(excp.Message);
            }

            return null;
        }

        private bool CheckCardAmount(List<CardInfo> apiRes, int maxAmount)
        {
            if (apiRes.Count > maxAmount) return false;
            return true;
        }

        public virtual async Task<CardInfo?> FecthAPI((Point, Point) cardTitleRect, (Point, Point) cardSplashRect, ImageHash hash, (Point, Point)? cardDescRect = null)
        {
            (Point, Point) area;
            int splashSize = SplashSizes;
            _ = new Bitmap(splashSize, splashSize);
            area = cardTitleRect;
            Bitmap bm = TakeScreenshotFromArea(area);

            // Check is the card is still the same
            area = cardSplashRect;
            if (!CheckSplashCardTextValidity(area, hash))
            {
                bm.Dispose();
                return null;
            }
            CardInfo? card;
            try
            {
                card = await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Light);
                card ??= await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Moderate);
                card ??= await CheckCardName(bm, TextProcessing.CardText.Trim_aggressiveness.Aggresive);
            }
            catch (CardNameIsChangedException)
            {
                if (cardDescRect == null) return null;
                area = ((Point, Point))cardDescRect;
                bm = TakeScreenshotFromArea(area);
                card = await CheckCardDescription(bm);
                card ??= new()
                {
                    CardNameIsChanged = true
                };
                Debug.Write("This card has it's name changed! Therefore ethe analysis may not work.");
                return card;
            }

            if (card == null) return null;
            bm.Dispose();

            return card;
        }
    }
}
