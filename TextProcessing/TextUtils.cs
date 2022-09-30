using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.TextProcessing
{
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
                    if (!lookup.Contains(c))
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }

        public static (List<string> matches, string rest) GetMatchingSentencesFromText(string str, string word, string[]? blackList)
        {
            List<string> ListOfTermVectors = new();
            var (matchedString, rest) = GetSubStringsCointainingWord(StripSpecialCharacters(str.ToLower(), ":;[],"), word, blackList);

            foreach (string match in matchedString)
            {
                ListOfTermVectors.Add(match);
            }

            return (ListOfTermVectors, rest);
        }

        public static (string[] matchedString, string rest) GetSubStringsCointainingWord(string text, string word, string[]? blackList)
        {
            string[] rest = new string[0];
            string[] results = new string[0];
            // TODO: change 
            foreach (string sentence in text.Split('.'))
            {
                if (sentence.ToLower().Contains(word) && (blackList == null || !blackList.Any(sentence.ToLower().Contains)))
                {
                    Array.Resize(ref results, results.Length + 1);
                    results[results.Length - 1] = sentence;
                }
                else
                {
                    Array.Resize(ref rest, rest.Length + 1);
                    rest[^1] = sentence;
                }
            }
            return (results, string.Join(".", rest));
        }

        public static string[] FileParser(string dir)
        {
            return File.ReadAllLines(dir).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToArray();
        }
    }
}
