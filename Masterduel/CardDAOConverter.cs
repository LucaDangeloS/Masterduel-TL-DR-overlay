using static Masterduel_TLDR_overlay.Masterduel.CardInfo;

namespace Masterduel_TLDR_overlay.Masterduel
{
    /// <summary>
    /// This class is static.
    /// </summary>
    internal static class CardDAOConverter
    {
        // Public methods
        public static CardInfoDB ConvertToDbCardInfo(CardInfo card)
        {
            string convertedSplashHash = card.Splash.HashedSplash.Aggregate("", (prev, next) => prev + (next ? "1" : "0"));
            int splashHashSum = card.Splash.HashedSplash.Count(x => x);
            List<EffectDB> effectsArray = ConvertEffectsToArray(card);
            // Add the card name and description later
            SplashDB splash = new()
            {
                SplashHash = convertedSplashHash,
                SplashHashSum = splashHashSum,
                MeanBrightness = card.Splash.MeanBrightness,
                MeanSaturation = card.Splash.MeanSaturation
            };
            CardInfoDB dbCard = new()
            {
                Name = card.Name,
                Effects = effectsArray,
                Splash = new List<SplashDB>() { splash }
            };

            return dbCard;
        }
        public static CardInfo ConvertToCardInfo(CardInfoDB dbCard)
        {
            CardInfo card = new();
            SplashDB firstSplash = dbCard.Splash.First();
            card.Splash.HashedSplash = ConvertStringToSplashHash(firstSplash.SplashHash);
            card.Splash.MeanSaturation = firstSplash.MeanSaturation;
            card.Splash.MeanBrightness = firstSplash.MeanBrightness;
            card.Name = dbCard.Name;
            card.AddEffects(ConvertEffectsFromDB(dbCard.Effects));
            // Add the card name and description later
            return card;
        }

        //public  static List<EffectDB> ConvertEffectsToEffectsDB(List<Effect> effects)
        //{
        //    List<EffectDB> ret = new();

        //    foreach (Effect e in effects)
        //    {
        //        var effectDB = new EffectDB();
        //        effectDB.EffectType = e.Type.ToString();
        //        effectDB.EffectText = e.EffectString;
        //        ret.Add(effectDB);
        //    }

        //    return ret;
        //}

        // Private methods
        private static List<EffectDB> ConvertEffectsToArray(CardInfo card)
        {
            List<EffectDB> ret = new();
            
            foreach (Effect effect in card.Effects)
            {
                var effectDB = new EffectDB();
                effectDB.EffectType = effect.Type.ToString();
                effectDB.EffectText = effect.EffectString;
                ret.Add(effectDB);
            }

            return ret;
        }
        //private static string[] ConvertEffectsToArray(CardInfo card)
        //{
        //    string[] ret = new string[card.Effects.Count * 2];
        //    int i = 0;

        //    foreach (Effect effect in card.Effects)
        //    {
        //        ret[i] = effect.Type.ToString();
        //        ret[i + 1] = effect.EffectString;
        //        i++;
        //    }

        //    return ret;
        //}
        private static List<Effect> ConvertEffectsFromDB(List<EffectDB> effects)
        {
            List<Effect> finalData = new();

            foreach (EffectDB effect in effects)
            {
                Enum.TryParse(effect.EffectType, out Effect.EffectType type);
                finalData.Add(new Effect(type, effect.EffectText));
            }

            return finalData;
        }
        //private static List<Effect> ConvertEffectsFromString(string[] effects)
        //{
        //    List<Effect> finalData = new();
        //    int len = effects.Length;

        //    for (int i = 0; i < len; i++)
        //    {
        //        Enum.TryParse<Effect.EffectType>(effects[i], out Effect.EffectType type);
        //        finalData.Add(new Effect(type, effects[i + 1]));
        //    }

        //    return finalData;
        //}
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
