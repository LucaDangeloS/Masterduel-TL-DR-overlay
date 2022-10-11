using Masterduel_TLDR_overlay.Masterduel;
using Masterduel_TLDR_overlay.Screen;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System.Diagnostics;
using static Masterduel_TLDR_overlay.Masterduel.CardInfo;

namespace Masterduel_TLDR_overlay.Caching
{
    internal class LocalDB
    {
        private readonly string path = "db.sqlite3";
        private readonly string dir = "cache/";
        public readonly (int, int) SplashSize = (24, 24);
        private readonly float PrecisionSplashQuery = 0.10f;
        private SQLiteConnection connection;

        // Public methods
        public LocalDB()
        {
            CreateDbFolder();
            connection = new SQLiteConnection($"{dir}{path}");
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
                // Fix bug, all splashes added are the same
                var tmp = CardDAOConverter.ConvertToDbCardInfo(card);
                SetCardIds(ref tmp, cardObject.Id);
                var tmpSplash = tmp.Splash.First();
                tmpSplash.Card = cardObject;
                AddSplash(tmpSplash);
                return;
            }
            cardObject = CardDAOConverter.ConvertToDbCardInfo(card);
            connection.InsertWithChildren(cardObject);
        }

        /// <summary>
        /// Retrieves the card with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards with the best match with the minimum level of precision.
        /// Returns null if no cards meet the minimum similarity level.</returns>
        public CardInfo? GetCardBySplash(List<bool> splashHash, float precision = 0.94f)
        {
            int hashSum = splashHash.Count(x => x);
            int maxDiff = (int)(hashSum * PrecisionSplashQuery);
            var queryRes = connection.Query<SplashDB>("SELECT s.*, abs(SplashHashSum - ?) as sumdiff " +
                "FROM splashes s " +
                "WHERE sumdiff <= ? " +
                "ORDER BY sumdiff ASC",
                hashSum, maxDiff);

            return GetBestMatchingResult(splashHash, queryRes, precision);
        }
        
        public CardInfo? GetCardByName(string name)
        {
            var queryRes = connection.Query<CardInfoDB>("SELECT c.* " +
                "FROM cards c " +
                "WHERE c.Name = ? ",
                name);
            if (queryRes == null || queryRes.Count == 0)
                return null;
            
            CardInfoDB bestMatch = queryRes[0];
            connection.GetChildren(bestMatch);

            return CardDAOConverter.ConvertToCardInfo(bestMatch);
        }
        
        /// <summary>
        /// Retrieves the cards with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards that match the hash by the precision specified, 
        /// ordered in descending order of similarity.</returns>
        public List<CardInfo> GetCards(List<bool> splashHash, float precison = 0.94f)
        {
            List<CardInfo> matchedCards = new();
            //Implement
            return matchedCards;
        }

        // Private methods
        private void AddSplash(SplashDB splash)
        {
            connection.InsertWithChildren(splash);
        }
        private void CreateDbFolder()
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        private void SetCardIds(ref CardInfoDB card, int cardId)
        {
            card.Splash.ForEach((x) => x.CardId = cardId);
            card.Effects.ForEach((x) => x.CardId = cardId);
        }
        private CardInfoDB? SearchForCard(string cardName)
        {
            List<CardInfoDB> cards = connection.Query<CardInfoDB>(
                "SELECT * " +
                "FROM cards c " +
                "WHERE c.Name = ?", cardName);

            if (cards.Count == 0) return null;

            CardInfoDB? matchedCard = cards.First();
            connection.GetChildren(matchedCard, true);

            return matchedCard;
        }
        private void CreateTables()
        {
            connection.CreateTable<CardInfoDB>();
            connection.CreateTable<EffectDB>();
            connection.CreateTable<SplashDB>();
        }
        private CardInfo? GetBestMatchingResult(List<bool> splashHash, List<SplashDB> splashArts, float precision)
        {
            if (splashHash.Count == 0 || splashArts == null || splashArts.Count == 0) return null;
            // TODO: Make iterate for each splash
            SplashDB? bestMatch = null;
            float max = 0.0f;

            foreach (SplashDB c in splashArts)
            {
                float tmpPrec = Screen.ImageProcessing.CompareImages(splashHash, CardDAOConverter.ConvertStringToSplashHash(c.SplashHash));
                if (tmpPrec >= precision && tmpPrec > max)
                {
                    max = tmpPrec;
                    bestMatch = c;
                }
            }

            if (bestMatch == null)
                return null;

            connection.GetChildren(bestMatch, true);
            return CardDAOConverter.ConvertToCardInfo(bestMatch.Card);
        }
    }
}
