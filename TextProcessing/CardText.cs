using Catalyst;
using Masterduel_TLDR_overlay.Api;
using Masterduel_TLDR_overlay.Masterduel;
using Mosaik.Core;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                public static string NEGATION = @"(?:(?:when|(Quick Effect|opponent's|either player's)|if|face-up|the effects of){1,2}(?:[^.]*?|[^.]*?\.{1}.{0,30}?)|,[ ]?|:[ ]?|;[ ]?|^|\. )(?<!to |cannot be |but |but its effects are |but it has its effects |but its effect\(s\) is |was )(negate[d]?)(?:[^.])*";
                public static string[] NEGATION2 = { "activation", "that", "opponent's", "opponent", "activated", "when" };
                public static string[] NON_NEGATION2 = { "special", "summon", "its", "but", "are" };
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
            
            var FalseVec = new Dictionary<string, int>();
            var TrueVec = new Dictionary<string, int>();
            var QueryVec = new Dictionary<string, int>();

            //load data from txt files
            string[] falseN = Similarity.FileParser(dir + "/FalseNegations.txt");
            string[] trueN = Similarity.FileParser(dir + "/TrueNegations.txt");
            string[] testN = TextUtils.ExtractMultipleEffects(Similarity.FileParser(dir + "/TestNegations.txt"));

            // Get term vectors from files
            TrueVec = Similarity.GetTermVector(trueN);
            FalseVec = Similarity.GetTermVector(falseN);

            
            List<bool> testFileTarget = new();
            int i = 1;
            foreach (string cardText in testN)
            {
                QueryVec = Similarity.GetTermVector(cardText);
                Debug.Write(i + " ->\r\nTrue Negation score: ");
                var b1 = Similarity.CosineSimilarity(QueryVec, TrueVec);
                Debug.WriteLine(b1);
                Debug.Write("False Negation score: ");
                var b2 = Similarity.CosineSimilarity(QueryVec, FalseVec);
                Debug.WriteLine(b2);
                Debug.WriteLine("\r\n");

                testFileTarget.Add(b1 > b2);
                i++;
            }
            Debug.WriteLine(Similarity.CalculatePrecision(testFileTarget, testN));
        }

        public class TextUtils
        {
            static public string[] ExtractMultipleEffects(string[] lines)
            {
                string[] ret = new string[0];
                string buffer = "";
                int counter = 0;

                foreach (string s in lines)
                {
                    if (counter == 0)
                    {
                        buffer = s;
                    }
                    counter++;

                    foreach (string s2 in Similarity.GetSubStringsCointainingWord("negate", s))
                    {
                        Array.Resize(ref ret, ret.Length + 1);
                        ret[^1] = buffer + " " + s2;
                    }
                }
                return ret;
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
                    catch (System.FormatException e)
                    {
                        throw new Exception("Test file format not valid. First word in each line must be either 'true' or 'false'");
                    }
                }
                
                // return the similarity coefficient between compVec and resVec
                float precision = 0;
                for (int i = 0; i < resVec.Count; i++)
                {
                    if (resVec[i] == compVec[i])
                        precision++;
                }
                return precision / resVec.Count;
            }

            public static Dictionary<string, int> GetTermVector(string[] str)
            {
                var dict = new Dictionary<string, int>();

                foreach (string line in str)
                {
                    foreach (string n in GetSubStringsCointainingWord("negate", line))
                    {
                        addToDict(StripSpecialCharacters(n.ToLower(), "():;[]"), ref dict);
                        //Debug.WriteLine(n);
                    }
                }

                return dict;
            }

            public static Dictionary<string, int> GetTermVector(string str)
            {
                var dict = new Dictionary<string, int>();

                addToDict(StripSpecialCharacters(str.ToLower(), "():;[]"), ref dict);

                return dict;
            }

            public static string[] GetSubStringsCointainingWord(string word, string text)
            {
                string[] temp = new string[0];
                // TODO: change to a regex
                foreach (string sentence in text.Split('.'))
                {
                    if (sentence.ToLower().Contains(word))
                    {
                        Array.Resize(ref temp, temp.Length + 1);
                        temp[^1] = sentence;
                    }
                }
                return temp;
            }

            public static Dictionary<string, double> GetIdfVector(Dictionary<string, int> ocurrences, int ColSize)
            {
                var idfVector = new Dictionary<string, double>();
                foreach (string word in ocurrences.Keys)
                {
                    idfVector[word] = 1 + Math.Log(ColSize/ocurrences[word]);
                }
                return idfVector;
            }

            public static string[] FileParser(string dir)
            {
                return File.ReadAllLines(dir).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToArray();
            }

            // Normalize TF, divide term ocurrence in card section divided by total ocurrences of term in collection
        }

    }
}
