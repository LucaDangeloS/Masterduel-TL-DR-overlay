﻿using Masterduel_TLDR_overlay.Masterduel;
using System.Diagnostics;
using System.Text;
using static Masterduel_TLDR_overlay.Masterduel.CardInfo;

namespace Masterduel_TLDR_overlay.TextProcessing
{
    /// <summary>
    ///    This is a sttatic class.
    /// </summary>
    internal static class CardText
    {
        // Public methods

        /// <summary>
        /// Trims and cleans up the card name string to remove missrecognized characters and artifacts from the OCR algorithm.
        /// </summary>
        /// <param name="name">The card raw name.</param>
        /// <param name="aggressiveness">The aggressiveness parameter determines how many characters will be trimmed from both sides of the string.</param>
        /// <returns>Returns the string with the cleaned up and trimmed card name.</returns>
        static public string TrimCardName(string name, Trim_aggressiveness aggressiveness)
        {
            StringBuilder sb = new();
            int agr_int = (int)aggressiveness;
            
            sb.Append(TextUtils.StripSpecialCharacters(name, "#='[]();|_"));
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
                return TextUtils.StripSpecialCharacters(str[..1], ":!") + 
                    str[1..(len-1)] + TextUtils.StripSpecialCharacters(str.Substring(len - 1, 1), ":!");
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

        static public CardInfo GetDescFeatures(CardInfo card)
        {
            // Negations
            var (matches, rest) = TextUtils.GetMatchingSentencesFromText(card.Desc, "negate", TextRules.FALSE_NEGATIONS);
            var negations = GetCardNegations(matches);
            card.AddEffects(negations);

            // TODO: Add the rest of effects

            return card;
        }

        // Private methods
        static private void AddToDict(string str, ref Dictionary<string, int> dict)
        {
            foreach (string word in str.Split(" "))
            {
                if (dict.ContainsKey(word))
                    dict[word]++;
                else
                    dict[word] = 1;
            }
        }

        private class TextRules
        {
            public static string[] FALSE_NEGATIONS = { "cannot be negated", "was negated" };

            public static string[] TRUE_IMMUNITIES =
            {
                "immune to",
                "unaffected by",
                "cannot be affected by",
                "cannot be destroyed",
                "cannot target",
                "cannot be targeted"
            };
            public static string[] TRUE_QUICK_EFFECTS =
            {
                "quick effect",
                "during your opponent",
                "during either player"
            };
        }

        private static List<Effect> GetCardNegations(List<string> matches)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            Dictionary<string, int> QueryVec;
            List<Effect> effectsList = new();

            string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
            string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");

            // Get term vectors from files
            var TrueVec = GetTermVectorFromFile(trueN);
            var FalseVec = GetTermVectorFromFile(falseN);

            // Check for negates
            foreach (string sentence in matches)
            {
                QueryVec = GetTermVector(sentence);
                var trueCoeff = Similarity.CosineSimilarity(QueryVec, TrueVec);
                var falseCoeff = Similarity.CosineSimilarity(QueryVec, FalseVec);

                if (trueCoeff > falseCoeff)
                {
                    effectsList.Add(new Effect(Effect.EffectType.NEGATION, sentence));
                }
            }
            return effectsList;
        }

        private static Dictionary<string, int> GetTermVectorFromFile(string[] str)
        {
            var dict = new Dictionary<string, int>();

            foreach (string line in str)
            {
                AddToDict(TextUtils.StripSpecialCharacters(line.ToLower(), ":;[],.-"), ref dict);
            }
            
            return dict;
        }
        private static Dictionary<string, int> GetTermVector(string str)
        {
            var dict = new Dictionary<string, int>();

            foreach (string word in str.Split(" "))
            {
                AddToDict(word, ref dict);
            }

            return dict;
        }

        protected class Similarity
        {
            // Public methods
            public static double CosineSimilarity(Dictionary<string, int> vec1, Dictionary<string, int> vec2)
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

            // Private methods
            // Rewrite
            static public void TestModel()
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;

                var FalseVec = new Dictionary<string, int>();
                var TrueVec = new Dictionary<string, int>();
                var QueryVec = new Dictionary<string, int>();

                //load data from txt files
                string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");
                string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
                string[] testN = TextUtils.FileParser(dir + "/TestNegations.txt");

                // Get term vectors from files
                TrueVec = GetTermVectorFromFile(trueN);
                FalseVec = GetTermVectorFromFile(falseN);


                List<bool> testFileTarget = new();
                int i = 1;
                foreach (string cardText in testN)
                {
                    QueryVec = GetTermVector(cardText);
                    Debug.Write(i + " ->\r\nTrue Negation score: ");
                    var b1 = CosineSimilarity(QueryVec, TrueVec);
                    Debug.WriteLine(b1);
                    Debug.Write("False Negation score: ");
                    var b2 = CosineSimilarity(QueryVec, FalseVec);
                    if (cardText.Contains(TextRules.FALSE_NEGATIONS[0]))
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
                Debug.WriteLine(CalculatePrecision(testFileTarget, testN));
            }

            private static float CalculatePrecision(List<bool> resVec, string[] file)
            {
                List<bool> compVec = new();
                
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
        static private void PrintDict<T>(IDictionary<string, T> dict)
        {
            var temp = from entry in dict orderby entry.Value ascending select entry;
            var sortedDict = temp.ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (string word in sortedDict.Keys)
            {
                Debug.WriteLine(word + " : " + dict[word] + "\r\n");
            }
        }
    }
}
