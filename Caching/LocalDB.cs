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
            CardInfoDB? cardObject = SearchForCard(card);

            if (cardObject != null)
            {
                AddSplash(CardDAOConverter.ConvertToDbCardInfo(card).Splash.First(), cardObject.Id);
                return;
            }
            cardObject = CardDAOConverter.ConvertToDbCardInfo(card);
            connection.InsertWithChildren(cardObject);
        }
        //private void AddSplashes(List<Effect> effects, int cardId)
        //{
        //    // Implement
        //    List<EffectDB> dbEffects = CardDAOConverter.ConvertEffectsToEffectsDB(effects);
        //    dbEffects.ForEach((e) => e.CardId = cardId);

        //    connection.InsertAllWithChildren(dbEffects);
        //}
        /// <summary>
        /// Retrieves the card with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards with the best match with the minimum level of precision.
        /// Returns null if no cards meet the minimum similarity level.</returns>
        public CardInfo? GetCard(List<bool> splashHash, float precision = 0.94f)
        {
            int hashSum = splashHash.Count(x => x);
            int maxDiff = (int)(hashSum * 0.15f);
            var queryRes = connection.Query<CardInfoDB>("SELECT CardId, abs(SplashHashSum - ?) as sumdiff " +
                "FROM splashes WHERE sumdiff <= ? " +
                "ORDER BY sumdiff ASC", hashSum, maxDiff);

            return GetBestMatchingResult(splashHash, queryRes, precision); ;
        }
        public CardInfo? GetCard(List<bool> splashHash, float mBrightness, float mSaturation, float precision = 0.94f)
        {
            int hashSum = splashHash.Count(x => x);
            int maxDiff = (int) (hashSum * 0.15f);
            var queryRes = connection.Query<CardInfoDB>("SELECT c.*, abs(SplashHashSum - ?) as sumdiff, " +
                "abs(MeanBrightness - ?) as brightDiff, (MeanSaturation - ?) as satDiff " +
                "FROM cards c " +
                "JOIN splashes s ON c.Id=s.CardId " +
                "WHERE sumdiff <= ? AND brightDiff <= 0.04 AND satDiff <= 0.04 " +
                "ORDER BY sumdiff ASC",
                hashSum, mBrightness, mSaturation, maxDiff);
            //TODO: Do something regarding the saturation and brightness

            if (queryRes == null || queryRes.Count == 0)
            {
                return null;
            };
            return GetBestMatchingResult(splashHash, queryRes, precision);
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
        private void AddSplash(SplashDB splash, int cardId)
        {
            splash.CardId = cardId;
            connection.InsertWithChildren(splash);
        }
        private void CreateDbFolder()
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        private CardInfoDB? SearchForCard(CardInfo queryCard)
        {
            List<CardInfoDB> cards = connection.Query<CardInfoDB>(
                "SELECT * " +
                "FROM cards c " +
                "WHERE c.Name = ?", queryCard.Name);
            if (cards.Count == 0) return null;

            int cardId = cards.First().Id;

            cards = connection.Query<CardInfoDB>(
                "SELECT * " +
                "FROM cards c " +
                "WHERE c.Id = ?", cardId);
            return cards.FirstOrDefault();
        }
        private void CreateTables()
        {
            connection.CreateTable<CardInfoDB>();
            connection.CreateTable<EffectDB>();
            connection.CreateTable<SplashDB>();
        }
        private CardInfo? GetBestMatchingResult(List<bool> splashHash, List<CardInfoDB> cards, float precision)
        {
            if (splashHash.Count == 0 || cards == null || cards.Count == 0) return null;
            // TODO: Make iterate for each splash
            CardInfoDB bestMatch = cards.First();
            connection.GetChildren(bestMatch, true);
            CardInfo card = CardDAOConverter.ConvertToCardInfo(bestMatch);
            if (ScreenProcessing.CompareImages(splashHash, card.Splash.HashedSplash) >= precision)
            {
                return card;
            }
            return null;
        }
    }
}
