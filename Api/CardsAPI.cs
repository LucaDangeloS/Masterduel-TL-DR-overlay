using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using Masterduel_TLDR_overlay.Masterduel;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Masterduel_TLDR_overlay.Api.JsonCardResponse;

namespace Masterduel_TLDR_overlay.Api
{
    /// <summary>
    ///    This is a sttatic class.
    /// </summary>
    internal static class CardsAPI
    {
        private static readonly string BASE_URL = "https://db.ygoprodeck.com/api/v7/cardinfo.php";
        private static readonly HttpClient client = new();
        
        // Public methods

        /// <summary>
        /// Queries asynchronously the db.ygoprodeck database for a fuzzy search with the card name passed as parameter.
        /// </summary>
        /// <param name="cardName">Name of the card to search for.</param>
        /// <exception cref="NoCardsFoundException">Thrown if the card name yielded no results.</exception>
        /// <exception cref="HttpRequestException">Thrown if the request was unsuccesful.</exception>
        /// <returns>Returns a <see cref="Task"/> object with a <see cref="CardInfo"/> <see cref="List"/> for the cards found by the search.</returns>
        public static async Task<List<CardInfo>> GetCardByNameAsync(string cardName)
        {
            string fuzzyPath = BASE_URL + "/?fname=" +  cardName;
            HttpResponseMessage response = await client.GetAsync(fuzzyPath);
            response.EnsureSuccessStatusCode();
            JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
            if (res == null) throw new NoCardsFoundException("No cards were found with name: " + cardName);

            List<CardInfo> cards = new();

            foreach (JsonCardInfo jsonCard in res.Data)
            {
                cards.Add(CardAPIObjConverter.ConvertToCardInfo(jsonCard));
            }

            return cards;
        }

        public static async Task<List<CardInfo>> GetCardByExactNameAsync(string cardName)
        {
            string exactPath = BASE_URL + "/?name=" + cardName;
            HttpResponseMessage response = await client.GetAsync(exactPath);
            response.EnsureSuccessStatusCode();
            JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
            
            if (res == null) throw new NoCardsFoundException("No cards were found with name: " + cardName);

            List<CardInfo> cards = new ();
            
            foreach (JsonCardInfo jsonCard in res.Data)
            {
                cards.Add(CardAPIObjConverter.ConvertToCardInfo(jsonCard));
            }

            return cards;
        }
    }

    class JsonCardResponse
    {
        public List<JsonCardInfo> Data { get; set; }
    }
    public class JsonCardInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Desc { get; set; }
    }
    class CardAPIObjConverter
    {
        public static CardInfo ConvertToCardInfo(JsonCardInfo res)
        {
            return new CardInfo(res.Name, res.Desc);
        }
    }
    
    class NoCardsFoundException : Exception
    {
        public NoCardsFoundException(string message): base(message) { }
    }

}
