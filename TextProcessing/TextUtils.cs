using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.TextProcessing;

/// <summary>
///    This is a sttatic class.
/// </summary>
internal static class TextUtils
{
    // Public methods
    public static string StripSpecialCharacters(string str, string chars)
    {
        List<char> lookup = new List<char>(chars);
        StringBuilder sb = new();

        foreach (char c in str)
        {
            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= 0 && c <= 9 || c == ' ')
            {
                sb.Append(c);
            }
            else
            {
                if (!lookup.Contains(c) && c != '\n')
                {
                    sb.Append(c);
                }
            }
        }
        return sb.ToString();
    }
        
    /// <summary>
    /// This method is case sensitive.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="word"></param>
    /// <param name="blackList"></param>
    /// <returns></returns>
    public static (List<string> matches, string rest) GetMatchingSentencesFromText(string str, string word, string[]? blackList = null)
    {
        List<string> ListOfTermVectors = new();
        var (matchedString, rest) = GetSubStringsCointainingWord(StripSpecialCharacters(str, ":;[],"), word, blackList);

        foreach (string match in matchedString)
        {
            ListOfTermVectors.Add(match);
        }

        return (ListOfTermVectors, rest);
    }

    public static (List<string> matches, string rest) GetMatchingSentencesFromText(string str, string[] words, string[]? blackList = null)
    {
        List<string> ListOfTermVectors = new();
        var (matchedString, rest) = GetSubStringsCointainingWord(StripSpecialCharacters(str, ":;[],"), words, blackList);

        foreach (string match in matchedString)
        {
            ListOfTermVectors.Add(match);
        }

        return (ListOfTermVectors, rest);
    }

    /// <summary>
    /// This method is case sensitive.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="word"></param>
    /// <param name="blackList"></param>
    /// <returns></returns>
    public static (string[] matchedString, string rest) GetSubStringsCointainingWord(string text, string word, string[]? blackList)
    {
        string[] rest = Array.Empty<string>();
        string[] results = Array.Empty<string>();
        // TODO: change 
        foreach (string sentence in text.Split('.'))
        {                
            if (Regex.IsMatch(sentence, word) && (blackList == null || !blackList.Any(sentence.Contains)))
            {
                Array.Resize(ref results, results.Length + 1);
                results[^1] = sentence;
            }
            else
            {
                Array.Resize(ref rest, rest.Length + 1);
                rest[^1] = sentence;
            }
        }
        return (results, string.Join(".", rest));
    }
    public static (string[] matchedString, string rest) GetSubStringsCointainingWord(string text, string[] words, string[]? blackList)
    {
        string[] rest = Array.Empty<string>();
        string[] results = Array.Empty<string>();
        // TODO: change 
        foreach (string sentence in text.Split('.'))
        {
            if (words.Any((word) => Regex.IsMatch(sentence, word)) && (blackList == null || !blackList.Any(sentence.Contains)))
            {
                Array.Resize(ref results, results.Length + 1);
                results[^1] = sentence;
            }
            else
            {
                Array.Resize(ref rest, rest.Length + 1);
                rest[^1] = sentence;
            }
        }
        return (results, string.Join(".", rest));
    }
        
    public static bool IsStringInSentence(string sentence, string[] words)
    {
        for (int i = 0; i < words.Length; i++)
        {
            if (sentence.Contains(words[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsStringInSentence(string sentence, string word)
    {
        return sentence.Contains(word);
    }

    public static string[] FileParser(string dir)
    {
        return File.ReadAllLines(dir).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToArray();
    }
}
