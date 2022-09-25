using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Api
{
    internal static class CardsAPI
    {
        private static readonly string BASE_URL = "https://db.ygoprodeck.com/api/v7/cardinfo.php";
        private static readonly HttpClient client = new();

        /// <summary>
        /// Queries asynchronously the db.ygoprodeck database for a fuzzy search with the card name passed as parameter.
        /// </summary>
        /// <param name="cardName">Name of the card to search for.</param>
        /// <exception cref="NoCardsFoundException">Thrown if the card name yielded no results.</exception>
        /// <exception cref="HttpRequestException">Thrown if the request was unsuccesful.</exception>
        /// <returns>Returns a <see cref="Task"/> object with a <see cref="CardInfo"/> <see cref="List"/> for the cards found by the search.</returns>
        public static async Task<List<CardInfo>> GetCardByNameAsync(string cardName)
        {
            string path = BASE_URL + "/?fname=" +  cardName;
            HttpResponseMessage response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
            if (res == null) throw new NoCardsFoundException("No cards were found with name: " + cardName);
            return res.Data;
        }


    }

    public class CardInfo
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public CardInfo(string name, string desc)
        {
            Name = name;
            Desc = desc;
        }
        public override string ToString()
        {
            return "Name: " + Name
                +"\r\nDescription: " + Desc;
        }
    }
    class JsonCardResponse
    {
        public List<CardInfo> Data { get; set; }
    }
    class NoCardsFoundException : Exception
    {
        public NoCardsFoundException(string message): base(message) { }
    }

}
