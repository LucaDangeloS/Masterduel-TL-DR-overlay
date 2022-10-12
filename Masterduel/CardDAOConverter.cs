using static Masterduel_TLDR_overlay.Masterduel.CardInfo;
using static Masterduel_TLDR_overlay.Screen.ImageProcessing;

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
            string convertedSplashHash = card.Splash.HashedSplash.Hash.Aggregate("", (prev, next) => prev + (next ? "1" : "0"));
            int splashHashSum = card.Splash.HashedSplash.HashSum;
            // Add the card name and description later
            SplashDB splash = new()
            {
                SplashHash = convertedSplashHash,
                SplashHashSum = splashHashSum,
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
            card.Name = dbCard.Name;
            card.AddEffects(ConvertEffectsFromDB(dbCard.Effects));
            // Add the card name and description later
            return card;
        }

        public static ImageHash ConvertStringToSplashHash(string splashHash)
        {
            List<bool> hash = new();
            Size size = new((int)Math.Sqrt(hash.Count), (int)Math.Sqrt(hash.Count));

            foreach (char c in splashHash)
            {
                hash.Add(c == '1');
            }

            return new ImageHash(hash, size);
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
