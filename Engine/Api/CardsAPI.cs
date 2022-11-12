using System.Net.Http.Json;
using TLDROverlay.Masterduel;
using TLDROverlay.Exceptions;

namespace TLDROverlay.Api;

/// <summary>
///    This is a static class.
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
        string fuzzyPath = BASE_URL + "/?sort=name&fname=" + querifyURI(cardName);
        HttpResponseMessage response = await client.GetAsync(fuzzyPath);
        response.EnsureSuccessStatusCode();
        JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
        if (res == null) throw new NoCardsFoundException(cardName);

        List<CardInfo> cards = new();

        foreach (JsonCardInfo jsonCard in res.Data)
        {
            cards.Add(CardAPIObjConverter.ConvertToCardInfo(jsonCard));
        }
        if (cards.Count == 0) throw new NoCardsFoundException(cardName);
        return cards;
    }

    public static async Task<List<CardInfo>> GetCardByExactNameAsync(string cardName)
    {
        string exactPath = BASE_URL + "/?sort=name&name=" + querifyURI(cardName);
        HttpResponseMessage response = await client.GetAsync(exactPath);
        response.EnsureSuccessStatusCode();
        JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
            
        if (res == null) throw new NoCardsFoundException(cardName);

        List<CardInfo> cards = new ();
            
        foreach (JsonCardInfo jsonCard in res.Data)
        {
            cards.Add(CardAPIObjConverter.ConvertToCardInfo(jsonCard));
        }
        if (cards.Count == 0) throw new NoCardsFoundException(cardName);
            
        return cards;
    }
    
    public static async Task<List<CardInfo>> GetCardDescAsync(string cardDesc)
    {
        string exactPath = BASE_URL + "/?sort=name&desc=" + querifyURI(cardDesc);
        HttpResponseMessage response = await client.GetAsync(exactPath);
        response.EnsureSuccessStatusCode();
        JsonCardResponse? res = await response.Content.ReadFromJsonAsync<JsonCardResponse>();
        List<CardInfo> cards = new();

        if (res == null) return cards;


        foreach (JsonCardInfo jsonCard in res.Data)
        {
            cards.Add(CardAPIObjConverter.ConvertToCardInfo(jsonCard));
        }

        return cards;
    }

    //public static async Task<List<CardInfo>> GetCardByIntersectionOfNames(string cardName)
    //{
    //    string exactPath = BASE_URL + "/?sort=name&name=" + cardName;
    //    // Apparently names between & are treated as separate queries
    //}

    public static async Task<List<CardInfo>> TryGetCardNameAsync(string cardName)
    {
        List<CardInfo> response;
        try
        {
            response = await GetCardByExactNameAsync(cardName);
            return response;
        }
        catch (Exception)  {}

        response = await GetCardByNameAsync(cardName);
        
        return response;
    }

    // Private methods
    //private static string InteresectResponses(List<>)
    //{

    //}
    
    private static string querifyURI(string uri)
    {
        string queryString = uri;
        queryString = queryString.Replace("&", "%26");

        return queryString;
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
