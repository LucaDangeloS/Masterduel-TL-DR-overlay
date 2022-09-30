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

        public class SummarizedData
        {
            public struct Effect
            {
                public enum EffectType
                {
                    NEGATION,
                    DESTRUCTION,
                    BANISH,
                    ON_DEATH,
                    UNTARGETEABLE,
                    INMUNITY
                }
                public EffectType Type { get; set; }
                public string EffectString;
                public Effect(EffectType type, string effectString)
                {
                    Type = type;
                    EffectString = effectString;
                }
                public override string ToString() => $"{Type}: {EffectString}";
            }
            public SummarizedData() { this.Effects = new(); }
            public List<Effect> Effects { get; set; }
            public void AddEffect(Effect effect) { Effects.Add(effect); }
            public void AddEffects(List<Effect> effects) { Effects.AddRange(effects); }
            public List<Effect>? GetEffects() { return Effects.Count > 0 ?  Effects : null; }
        }
    }
}
