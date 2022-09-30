using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Masterduel_TLDR_overlay.Masterduel.CardInfo;

namespace Masterduel_TLDR_overlay.Masterduel
{
    /// <summary>
    /// This class is static.
    /// </summary>
    internal static class CardDAOConverter
    {
        // Public methods
        public static CardInfoDAO ConvertToDbCardInfo(SummarizedData summarizedData, List<bool> splashHash)
        {
            string convertedSplashHash = splashHash.Aggregate("", (prev, next) => prev + (next ? "1" : "0"));
            int splashHashSum = splashHash.Count(x => x);
            string[] effectsArray = ConvertEffectsToString(summarizedData);

            CardInfoDAO dbCard = new(convertedSplashHash, effectsArray, splashHashSum);

            return dbCard;
        }
        public static (SummarizedData effectData, List<bool> splashHash) ConvertToSummarizedData(CardInfoDAO dbCard)
        {
            SummarizedData cardEffect = new();
            List<bool> splashData = ConvertStringToSplashHash(dbCard.SplashHash);
            cardEffect.AddEffects(ConvertEffectsFromString(dbCard.EffectArray));
            
            return (cardEffect, splashData);
        }

        // Private methods
        private static string[] ConvertEffectsToString(SummarizedData summarizedData)
        {
            string[] ret = new string[summarizedData.Effects.Count * 2];
            int i = 0;
            
            foreach (SummarizedData.Effect effect in summarizedData.Effects)
            {
                ret[i] = effect.Type.ToString();
                ret[i + 1] = effect.EffectString;
                i++;
            }

            return ret;
        }
        private static List<SummarizedData.Effect> ConvertEffectsFromString(string[] effects)
        {
            List<SummarizedData.Effect> finalData = new();
            int len = effects.Count();

            for (int i = 0; i < len; i++)
            {
                Enum.TryParse<SummarizedData.Effect.EffectType>(effects[i], out SummarizedData.Effect.EffectType type);
                finalData.Add(new SummarizedData.Effect(type, effects[i + 1]));
            }

            return finalData;
        }
        private static List<bool> ConvertStringToSplashHash(string splashHash)
        {
            List<bool> ret = new();
            
            foreach (char c in splashHash)
            {
                ret.Add(c == '1' ? true : false);
            }
            
            return ret;
        }
    }
}
