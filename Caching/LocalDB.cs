using System.Data.SQLite;
using Masterduel_TLDR_overlay.Masterduel;
using static Masterduel_TLDR_overlay.Masterduel.CardInfo;

namespace Masterduel_TLDR_overlay.Caching
{
    internal class LocalDB
    {
        private readonly string path = "db.sqlite3";
        private readonly string dir = "cache/";
        private SQLiteConnection connection;

        // Public methods
        public LocalDB()
        {
            CreateDbFolder();
            connection = new SQLiteConnection($"Data Source={dir}{path};Version=3;");
        }
        public void Initialize()
        {
            connection.Open();
            CreateTables();
            connection.Close();            
        }
        public bool AddCard(SummarizedData summarizedData, List<bool> splashHash)
        {
            connection.Open();
            CardInfoDAO cardObject = CardDAOConverter.ConvertToDbCardInfo(summarizedData, splashHash);
            bool result = AddCardToDb(cardObject);
            connection.Close();
            return result;
        }
        /// <summary>
        /// Retrieves the card with the best match on their splash.
        /// </summary>
        /// <param name="splashHash">The hash of the splash art from the card.</param>
        /// <param name="precison">Number from 0 to 1 indicating the precision.</param>
        /// <returns>Returns the cards with the best match with the minimum level of precision.
        /// Returns null if no cards meet the minimum similarity level.</returns>
        public CardInfo? GetCard(List<bool> splashHash, float precison = 0.94f)
        {
            connection.Open();
            //Implement
            connection.Close();
            return null;
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
            connection.Open();
            //Implement
            connection.Close();
            return matchedCards;
        }

        // Private methods
        private void CreateDbFolder()
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        private void CreateTables()
        {
            using var command = new SQLiteCommand(connection);
            // Implement
            command.CommandText = "CREATE TABLE IF NOT EXISTS cards (id INTEGER PRIMARY KEY, name TEXT, image BLOB)";
            command.ExecuteNonQuery();
        }
        private bool AddCardToDb(CardInfoDAO card)
        {
            // Implement
            return false;
        }
    }
}
