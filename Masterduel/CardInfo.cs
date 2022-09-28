using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterduel_TLDR_overlay.Masterduel
{
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
                + "\r\nDescription: " + Desc;
        }
    }
}
