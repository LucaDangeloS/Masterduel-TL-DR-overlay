using Catalyst;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Masterduel;
using Mosaik.Core;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Catalyst.Models.FastText;

namespace Masterduel_TLDR_overlay.TextProcessing
{
    internal static class CardText
    {
        /// <summary>
        /// Trims and cleans up the card name string to remove missrecognized characters and artifacts from the OCR algorithm.
        /// </summary>
        /// <param name="name">The card raw name.</param>
        /// <param name="aggressiveness">The aggressiveness parameter determines how many characters will be trimmed from both sides of the string.</param>
        /// <returns>Returns the string with the cleaned up and trimmed card name.</returns>
        public static string TrimCardName(string name, Trim_aggressiveness aggressiveness)
        {
            StringBuilder sb = new();
            int agr_int = (int)aggressiveness;

            sb.Append(StripSpecialCharacters(name, "#='[]();|_"));

            string str = sb.ToString();
            int len = str.Length;

            if (len <= 14)
            {
                var new_agr_int = agr_int / 2;
                if (aggressiveness <= Trim_aggressiveness.Light || len <= new_agr_int * 4) { return str; }
                return str.Substring(new_agr_int, len - new_agr_int * 2);
            }
            if (aggressiveness == Trim_aggressiveness.Light)
            {
                return StripSpecialCharacters(str[..1], ":!") + str[1..len] + StripSpecialCharacters(str.Substring(len - 1, 1), ":!");
            }
            return str.Substring(agr_int, len - agr_int * 2);
        }

        public enum Trim_aggressiveness
        {
            None,
            Light,
            Moderate,
            Aggresive
        }

        private static string StripSpecialCharacters(string str, string chars)
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

        private static class CardRegex
        {
            public static class Monsters
            {
                public static string[] NEGATION2 = { "activation", "that", "opponent's", "opponent", "activated", "when" };
                public static string[] NON_NEGATION2 = { "special", "summon", "its", "but", "are" };
                public static string[] FALSE_NEGATIONS = { "cannot be negated"};
                // that do not have "is" or "was" before the "negated"
                // forward lookup for word "destroy", "banish"
                // asign importance function relating how close a word is to the keywords?
            }

        }

        static public void addToDict(string str, ref Dictionary<string, int> dict)
        {
            foreach (string word in str.Split(" "))
            {
                if (dict.ContainsKey(word))
                    dict[word]++;
                else
                    dict[word] = 1;
            }
        }
        static public void printDict<T>(IDictionary<string, T> dict)
        {
            var temp = from entry in dict orderby entry.Value ascending select entry;
            var sortedDict = temp.ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (string word in sortedDict.Keys)
            {
                Debug.WriteLine(word + " : " + dict[word] + "\r\n");
            }
        }
        static public void GetDescFeatures(CardInfo card)
        {
            string dir = @"C:\Users\Lucad\Desktop\Cosas\masterduel-tldr-overlay\Masterduel TLDR overlay\";
            var QueryVec = new Dictionary<string, int>();

            string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");
            string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
            var (matches, rest) = TextUtils.GetMatchingSentencesFromText(card.Desc, "negate", CardRegex.Monsters.FALSE_NEGATIONS);

            // Get term vectors from files
            var TrueVec = TextUtils.GetTermVectorFromFile(trueN);
            var FalseVec = TextUtils.GetTermVectorFromFile(falseN);
            
            // Check for negates
            foreach (string sentence in matches)
            {
                var trueCoeff = Similarity.CosineSimilarity(QueryVec, TrueVec);
                var falseCoeff = Similarity.CosineSimilarity(QueryVec, FalseVec);

                if (trueCoeff > falseCoeff)
                {
                    Debug.WriteLine($"{sentence} HAS NEGATION");
                }
            }
        }

        static public void TestModel()
        {
            string dir = @"C:\Users\Lucad\Desktop\Cosas\masterduel-tldr-overlay\Masterduel TLDR overlay\";

            var FalseVec = new Dictionary<string, int>();
            var TrueVec = new Dictionary<string, int>();
            var QueryVec = new Dictionary<string, int>();

            //load data from txt files
            string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");
            string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
            string[] testN = TextUtils.FileParser(dir + "/TestNegations.txt");

            // Get term vectors from files
            TrueVec = TextUtils.GetTermVectorFromFile(trueN);
            FalseVec = TextUtils.GetTermVectorFromFile(falseN);


            List<bool> testFileTarget = new();
            int i = 1;
            foreach (string cardText in testN)
            {
                QueryVec = TextUtils.GetTermVector(cardText);
                Debug.Write(i + " ->\r\nTrue Negation score: ");
                var b1 = Similarity.CosineSimilarity(QueryVec, TrueVec);
                Debug.WriteLine(b1);
                Debug.Write("False Negation score: ");
                var b2 = Similarity.CosineSimilarity(QueryVec, FalseVec);
                if (cardText.Contains(CardRegex.Monsters.FALSE_NEGATIONS[0]))
                {
                    testFileTarget.Add(false);
                }
                else
                {
                    testFileTarget.Add(b1 > b2);
                }
                Debug.WriteLine(b2);
                Debug.WriteLine("\r\n");


                i++;
            }
            Debug.WriteLine(Similarity.CalculatePrecision(testFileTarget, testN));
        }

        public class TextUtils
        {
            public static Dictionary<string, int> GetTermVectorFromFile(string[] str)
            {
                var dict = new Dictionary<string, int>();

                foreach (string line in str)
                {
                    addToDict(StripSpecialCharacters(line.ToLower(), ":;[],.-"), ref dict);
                }

                return dict;
            }

            public static Dictionary<string, int> GetTermVector(string str)
            {
                var dict = new Dictionary<string, int>();

                foreach (string word in str.Split(" "))
                {
                    addToDict(word, ref dict);
                }

                return dict;
            }

            public static (List<string> matches, string rest) GetMatchingSentencesFromText(string str, string word, string[]? blackList)
            {
                List<string> ListOfTermVectors = new();
                var (matchedString, rest) = GetSubStringsCointainingWord(StripSpecialCharacters(str.ToLower(), ":;[],."), word, blackList);

                foreach (string match in matchedString)
                {
                    ListOfTermVectors.Add(match);
                }

                return (ListOfTermVectors, rest);
            }

            public static (string[] matchedString, string rest) GetSubStringsCointainingWord(string word, string text, string[]? blackList)
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

        public class Similarity
        {
            static public double CosineSimilarity(Dictionary<string, int> vec1, Dictionary<string, int> vec2)
            {
                // get all words in both vec1 and vec2
                var allWords = vec1.Keys.Union(vec2.Keys);

                // get dot product
                var dotProduct = allWords.Sum(word => vec1.GetValueOrDefault(word) * vec2.GetValueOrDefault(word));
                // get magnitudes
                var magnitude1 = Math.Sqrt(vec1.Sum(x => x.Value * x.Value));
                var magnitude2 = Math.Sqrt(vec2.Sum(x => x.Value * x.Value));
                // get cosine similarity
                var cosine = dotProduct / (magnitude1 * magnitude2);

                return cosine;
            }

            static public float CalculatePrecision(List<bool> resVec, string[] file)
            {
                List<bool> compVec = new List<bool>();
                
                foreach (string line in file)
                {
                    string temp = line.Split(' ')[0];
                    try
                    {
                        compVec.Add(Boolean.Parse(temp));
                    }
                    catch (FormatException e)
                    {
                        throw new Exception("Test file format not valid. First word in each line must be either 'true' or 'false'");
                    }
                }

                // return the similarity coefficient between compVec and resVec
                float precision = 0;
                for (int i = 0; i < resVec.Count; i++)
                {
                    if (resVec[i] == compVec[i])
                    {
                        precision++;
                    } else
                    {
                        Debug.WriteLine($"Failed at line {i+1} expected {compVec[i]} but got {resVec[i]}");
                    }
                }
                return precision / resVec.Count;
            }

        }

    }
}
