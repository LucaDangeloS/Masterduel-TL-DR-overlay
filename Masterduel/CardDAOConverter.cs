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
                Effects = ConvertEffectsToDB(card.Effects),
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

        public static List<bool> ConvertStringToSplashHash(string splashHash)
        {
            List<bool> ret = new();

            foreach (char c in splashHash)
            {
                ret.Add(c == '1');
            }

            return ret;
        }

        // Private methods
        private static List<EffectDB> ConvertEffectsToDB(List<Effect> effects)
        {
            List<EffectDB> ret = new();
            
            foreach (Effect effect in effects)
            {
                var effectDB = new EffectDB();
                effectDB.EffectType = effect.Type.ToString();
                effectDB.EffectText = effect.EffectString;
                ret.Add(effectDB);
            }

            return ret;
        }
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
    }
}
