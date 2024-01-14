using TLDROverlay.Masterduel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static TLDROverlay.Masterduel.CardInfo;

namespace TLDROverlay.TextProcessing;

/// <summary>
///    This is a static class.
/// </summary>
public static class CardText
{
    private static readonly string dir = System.AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
    private static readonly string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");

    // Get term vectors from files
    private static readonly Dictionary<string, int> TrueVec = GetTermVectorFromFile(trueN);
    private static readonly Dictionary<string, int> FalseVec = GetTermVectorFromFile(falseN);

    // Public methods

    /// <summary>
    /// Trims and cleans up the card name string to remove missrecognized characters and artifacts from the OCR algorithm.
    /// </summary>
    /// <param name="name">The card raw name.</param>
    /// <param name="aggressiveness">The aggressiveness parameter determines how many characters will be trimmed from both sides of the string.</param>
    /// <returns>Returns the string with the cleaned up and trimmed card name.</returns>
    static public string TrimCardName(string name, Trim_aggressiveness aggressiveness)
    {
        // DEBUG
        Debug.WriteLine("Card name: " + name);
        if (aggressiveness == Trim_aggressiveness.None) return name;
            
        StringBuilder sb = new();
        int agr_int = (int)aggressiveness;
        int floor_agr_int = (int) Math.Ceiling((double)agr_int / 2);

        sb.Append(TextUtils.StripSpecialCharacters(name, "=[]();|_\\"));
        string str = sb.ToString().Replace('—', '-');
        int len = str.Length;

        if (len <= agr_int * 3)
        {
            return str;
        }
        int str_start_trim = 2;
        int str_end_trim = 2;
        str = TextUtils.StripSpecialCharacters(str[..str_start_trim], "#:!\\") +
                    str[str_start_trim..(len - str_start_trim)] + 
                    TextUtils.StripSpecialCharacters(str.Substring(len - str_end_trim, str_end_trim), "\\:!\\-=");
            
        switch (aggressiveness)
        {
            case Trim_aggressiveness.Light:
                return str;

            case Trim_aggressiveness.Moderate:
                //Regex re = new Regex("[a-zA-Z0-9, ]*([\\-= ]*)", RegexOptions.Multiline);
                //Match m = re.Match(str);
                //Group filteredStr = m.Groups[0];

                //if (filteredStr.Success)
                //{
                //    return filteredStr.Value.Trim();
                //}
                break;
                    
            case Trim_aggressiveness.Aggresive:
                //re = new Regex("[a-zA-Z0-9, ]*([\\-=# ]*)", RegexOptions.Multiline);
                //m = re.Match(str);
                //filteredStr = m.Groups[0];
                //if (filteredStr.Success)
                //{
                //    str = filteredStr.Value.Trim();
                //    len = str.Length;
                //}
                break;
        }
        if (str.Length > floor_agr_int + agr_int)
        {
            return str[floor_agr_int..(len - agr_int)];
        }
        else
        {
            return str;
        }
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
        string lowerDesc = card.Desc.ToLower();
            
        // Negations
        var (matches, rest) = TextUtils.GetMatchingSentencesFromText(lowerDesc, TextRules.TRUE_NEGATION, TextRules.FALSE_NEGATIONS);
        var negations = GetCardNegations(matches);
        card.AddEffects(negations);

        // Immunities
        (matches, rest) = TextUtils.GetMatchingSentencesFromText(rest, TextRules.TRUE_IMMUNITIES);
        var immunities = GetCardEffects(Effect.EffectType.INMUNITY, matches);
        card.AddEffects(immunities);

        // Banishes
        (matches, rest) = TextUtils.GetMatchingSentencesFromText(rest, TextRules.TRUE_BANISH);
        var banishes = GetCardEffects(Effect.EffectType.BANISH, matches);
        card.AddEffects(banishes);

        // Destruction
        (matches, rest) = TextUtils.GetMatchingSentencesFromText(rest, TextRules.TRUE_DESTRUCTIONS);
        var destructions = GetCardEffects(Effect.EffectType.DESTRUCTION, matches);
        card.AddEffects(destructions);

        // Take Control
            
        // On-Death

        // General Quick-Effects
        (matches, _) = TextUtils.GetMatchingSentencesFromText(rest, TextRules.TRUE_QUICK_EFFECTS);
        var quickEffects = GetCardEffects(Effect.EffectType.QUICK_EFFECT, matches);
        card.AddEffects(quickEffects);

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
        public static string TRUE_NEGATION = "negate";
        public static string[] FALSE_NEGATIONS = { "cannot be negated", "was negated" };
        public static string[] TRUE_IMMUNITIES =
        {
            "immune to",
            "unaffected by",
            "cannot be affected by",
            "cannot be destroyed",
            "cannot target",
            "cannot be targeted",
            "[^.]*?would be destroyed[^.]*?(instead|not)[^.]*",
        };
        public static string TRUE_BANISH = "((opponent|on the field)[^.]{0,30}?banish[^ed]|banish[^ed][^.]{0,30}?(opponent|on the field))";
        public static string[] TRUE_DESTRUCTIONS =
        {
            "((opponent|on the field)[^.]{0,30}?destroy[^eds]|destroy[^eds][^.]{0,30}?(opponent|on the field))"
        };
        public static string[] TRUE_QUICK_EFFECTS =
        {
            "quick effect",
            "during your opponent",
            "during either player",
            "when your opponent"
        };
    }

    private static List<Effect> GetCardNegations(List<string> matches)
    {
        Dictionary<string, int> QueryVec;
        List<Effect> effectsList = new();

        // Check for negates
        foreach (string sentence in matches)
        {
            QueryVec = GetTermVector(sentence);
            var trueCoeff = Similarity.CosineSimilarity(QueryVec, TrueVec);
            var falseCoeff = Similarity.CosineSimilarity(QueryVec, FalseVec);

            if (trueCoeff > falseCoeff)
            {
                effectsList.Add(new Effect(Effect.EffectType.NEGATION, sentence, true));
            }
        }
        return effectsList;
    }
        
    private static List<Effect> GetCardEffects(Effect.EffectType effectType, List<string> matches)
    {
        List<Effect> effectsList = new();

        foreach (string sentence in matches)
        {
            effectsList.Add(new Effect(effectType, sentence,
                effectType == Effect.EffectType.QUICK_EFFECT 
                || TextUtils.IsStringInSentence(sentence, TextRules.TRUE_QUICK_EFFECTS)));
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

            //load data from txt files
            string[] falseN = TextUtils.FileParser(dir + "/FalseNegations_stripped.txt");
            string[] trueN = TextUtils.FileParser(dir + "/TrueNegations_stripped.txt");
            string[] testN = TextUtils.FileParser(dir + "/TestNegations.txt");

            // Get term vectors from files
            Dictionary<string, int>? TrueVec = GetTermVectorFromFile(trueN);
            Dictionary<string, int>? FalseVec = GetTermVectorFromFile(falseN);


            List<bool> testFileTarget = new();
            int i = 1;
            foreach (string cardText in testN)
            {
                Dictionary<string, int>? QueryVec = GetTermVector(cardText);
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
                catch (FormatException)
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

