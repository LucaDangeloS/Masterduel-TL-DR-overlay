using SQLite;
using SQLiteNetExtensions.Attributes;
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
        public struct SplashInfo
        {
            public List<bool> HashedSplash { get; set; }
            public float MeanBrightness { get; set; }
            public float MeanSaturation { get; set; }
        }
        public SplashInfo Splash;
        
        // Constructors
        public CardInfo()
        {
            Name = "";
            Desc = "";
            Splash.HashedSplash = new List<bool>();
            Effects = new();
        }
        public CardInfo(string name, string desc)
        {
            Name = name;
            Desc = desc;
            Splash.HashedSplash = new List<bool>();
            Effects = new();
        }
        public CardInfo(string name, string desc, float meanBrightness, float meanSaturation)
        {
            Name = name;
            Desc = desc;
            Splash.HashedSplash = new List<bool>();
            Splash.MeanBrightness = meanBrightness;
            Splash.MeanSaturation = meanSaturation;
            Effects = new();
        }
        // Public methods
        public override string ToString()
        {
            return "Name: " + Name
                + "\r\nDescription: " + Desc;
        }
        public List<Effect> Effects { get; set; }
        public void AddEffect(Effect effect) { Effects.Add(effect); }
        public void AddEffects(List<Effect> effects) { Effects.AddRange(effects); }
        public void SetSplash(List<bool> hashedSplash, float meanBrightness, float meanSaturation)
        {
            Splash.HashedSplash = hashedSplash;
            Splash.MeanBrightness = meanBrightness;
            Splash.MeanSaturation = meanSaturation;
        }
        public List<Effect>? GetEffects() { return Effects.Count > 0 ? Effects : null; }

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
    }

    [Table("cards")]
    public class CardInfoDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<EffectDB> Effects { get; set; }
        [Indexed]
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SplashDB> Splash { get; set; }
    }
    [Table("effects")]
    public class EffectDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(CardInfoDB))]
        public int CardId { get; set; }
        public string EffectType { get; set; }
        
        [MaxLength(300)]
        public string EffectText { get; set; }

        [ManyToOne]
        public CardInfoDB Card { get; set; }
    }
    [Table("splashes")]
    public class SplashDB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(CardInfoDB))]
        public int CardId { get; set; }
        [Indexed]
        public string SplashHash { get; set; }
        [Indexed]
        public float MeanBrightness { get; set; }
        [Indexed]
        public float MeanSaturation { get; set; }
        [Indexed]
        public int SplashHashSum { get; set; }
        [ManyToOne]
        public CardInfoDB Card { get; set; }
    }
}
