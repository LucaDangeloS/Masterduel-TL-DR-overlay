using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            int agr_int = (int) aggressiveness;

            sb.Append(StripSpecialCharacters(name, "#='[]();|_"));

            string str = sb.ToString();
            int len = str.Length;

            if (len <= 14)
            {
                var new_agr_int = agr_int / 2;
                if (aggressiveness <= Trim_aggressiveness.Light || len <= new_agr_int * 4) { return str; }
                return str.Substring(new_agr_int, len - new_agr_int * 2);
            }
            return str.Substring(agr_int, len - agr_int*2);
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
    }
}
