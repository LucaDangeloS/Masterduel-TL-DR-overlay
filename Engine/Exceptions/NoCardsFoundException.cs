
namespace Masterduel_TLDR_overlay.Exceptions
{
    class NoCardsFoundException : Exception
    {
        private static readonly string DEFAULT_MESSAGE = "No good matches were returned for the card queried.";
        private readonly string QueryString;

        public NoCardsFoundException(string message, string query) : base(message)
        {
            QueryString = query;
        }
        public NoCardsFoundException(string query) : base(DEFAULT_MESSAGE)
        {
            QueryString = query;
        }

        public string GetQueryText()
        {
            return QueryString;
        }
    }
}
