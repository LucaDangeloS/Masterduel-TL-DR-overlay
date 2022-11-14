using TLDROverlay.Masterduel;
using TLDROverlay.Screen;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System.Diagnostics;
using static TLDROverlay.Masterduel.CardInfo;
using static TLDROverlay.Screen.ImageProcessing;
using TLDROverlay.Config;

namespace TLDROverlay.Caching
{
    internal class LocalDB
    {
        private readonly ConfigLoader _config = ConfigLoader.Instance;
        private readonly string path = "db.sqlite3";
        private readonly string dir = "cache/";
        public readonly (int, int) SplashSize;
        private readonly SQLiteConnection Connection;

        // Public methods
        public LocalDB()
        {
            SplashSize = (_config.GetIntProperty(ConfigMappings.SPLASH_SIZE), _config.GetIntProperty(ConfigMappings.SPLASH_SIZE));
            CreateDbFolder();
            Connection = new SQLiteConnection($"{dir}{path}");
        }
        
        public void Initialize()
        {
            CreateTables();
        }

        public void AddCard(CardInfo card)
        {
            CardInfoDB? cardObject = SearchForCard(card.Name);

            if (cardObject != null)
            {
                AddSplashToCard(card, cardObject);
                return;
            }
            cardObject = CardDAOConverter.ConvertToDbCardInfo(card);
            Connection.InsertWithChildren(cardObject);
        }

        /// <summary>
        /// Retrieves the card with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards with the best match with the minimum level of precision.
        /// Returns null if no cards meet the minimum similarity level.</returns>
        public CardInfo? GetCardBySplash(ImageHash splashHash, float precision)
        {
            int hashSum = splashHash.HashSum;
            int maxDiff = _config.GetIntProperty(ConfigMappings.MAX_PIXELS_DIFF);
            var queryRes = Connection.Query<SplashDB>(
                "SELECT s.*, abs(SplashHashSum - ?) as sumdiff " +
                "FROM splashes s " +
                "WHERE sumdiff <= ? " +
                "ORDER BY sumdiff ASC",
                hashSum, maxDiff);

            return GetBestMatchingResult(splashHash, queryRes, precision);
        }
        
        public CardInfo? GetCardByName(string name)
        {
            var queryRes = Connection.Query<CardInfoDB>(
                "SELECT c.* " +
                "FROM cards c " +
                "WHERE c.Name = ? ",
                name);
            if (queryRes == null || queryRes.Count == 0)
                return null;
            
            CardInfoDB bestMatch = queryRes.First();
            Connection.GetChildren(bestMatch);

            return CardDAOConverter.ConvertToCardInfo(bestMatch);
        }

        public void ClearDataBase()
        {
            Connection.DropTable<CardInfoDB>();
            Connection.DropTable<SplashDB>();
            CreateTables();
        }

        /// <summary>
        /// Retrieves the cards with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards that match the hash by the precision specified, 
        /// ordered in descending order of similarity.</returns>
        //public List<CardInfo> GetCards(ImageHash splashHash, float precision)
        //{
        //    List<CardInfo> matchedCards = new();
        //    //Implement
        //    return matchedCards;
        //}

        // Private methods
        private void AddSplashToCard(CardInfo card, CardInfoDB cardObject)
        {
            var tmp = CardDAOConverter.ConvertToDbCardInfo(card);
            var cardId = cardObject.Id;
            var tmpSplash = tmp.Splash.First();

            cardObject.Splash.ForEach((x) => x.CardId = cardId);
            cardObject.Effects.ForEach((x) => x.CardId = cardId);

            tmpSplash.Card = cardObject;
            Connection.InsertWithChildren(tmpSplash);
        }
        
        private void CreateDbFolder()
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        
        private CardInfoDB? SearchForCard(string cardName)
        {
            List<CardInfoDB> cards = Connection.Query<CardInfoDB>(
                "SELECT * " +
                "FROM cards c " +
                "WHERE c.Name = ?", cardName);

            if (cards.Count == 0) return null;

            CardInfoDB? matchedCard = cards.FirstOrDefault();
            if (matchedCard != null)
            {
                Connection.GetChildren(matchedCard, true);
            }
            return matchedCard;
        }
        
        private void CreateTables()
        {
            Connection.CreateTable<CardInfoDB>();
            Connection.CreateTable<EffectDB>();
            Connection.CreateTable<SplashDB>();
        }
        
        private CardInfo? GetBestMatchingResult(ImageHash splashHash, List<SplashDB> splashArts, float precision)
        {
            if (splashHash.HashSum == 0 || splashArts == null || splashArts.Count == 0) return null;
            // TODO: Make iterate for each splash
            SplashDB? bestMatch = null;
            float max = 0.0f;

            foreach (SplashDB c in splashArts)
            {
                float tmpPrec = splashHash.CompareTo(CardDAOConverter.ConvertStringToSplashHash(c.SplashHash));
                if (tmpPrec >= precision && tmpPrec >= max)
                {
                    max = tmpPrec;
                    bestMatch = c;
                }
            }

            if (bestMatch == null)
                return null;

            Connection.GetChildren(bestMatch, true);
            return CardDAOConverter.ConvertToCardInfo(bestMatch.Card);
        }
    }
}
