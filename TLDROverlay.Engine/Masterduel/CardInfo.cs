using SQLite;
using SQLiteNetExtensions.Attributes;
using static TLDROverlay.Screen.ImageProcessing;

namespace TLDROverlay.Masterduel;

public class CardInfo
{
    public bool CardNameIsChanged { get; set; } = false;
    public string Name { get; set; }
    public string Desc { get; set; }
    public struct SplashInfo
    {
        public ImageHash HashedSplash;
    }
    public SplashInfo Splash;
        
    // Constructors
    public CardInfo()
    {
        Name = "";
        Desc = "";
        Splash.HashedSplash = new ImageHash();
        Effects = new();
    }
    public CardInfo(string name, string desc)
    {
        Name = name;
        Desc = desc;
        Splash.HashedSplash = new ImageHash();
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
    public void SetSplash(ImageHash hashedSplash)
    {
        Splash.HashedSplash = hashedSplash;
    }
    public List<Effect>? GetEffects() { return Effects.Count > 0 ? Effects : null; }

    public struct Effect
    {
        public enum EffectType
        {
            NEGATION,
            INMUNITY,
            BANISH,
            DESTRUCTION,
            ON_DEATH,
            TAKE_CONTROL,
            QUICK_EFFECT
        }
        public EffectType Type { get; set; }
        public string EffectString;
        public bool QuickEffect { get; set; }
        public Effect(EffectType type, string effectString, bool isQuickEffect = false)
        {
            Type = type;
            EffectString = effectString;
            QuickEffect = isQuickEffect;
        }
        public override string ToString() => $"{Type}:\r\n {EffectString}"; // TODO: Capitalize

    }

    public override bool Equals(object? obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType())) return false;
        CardInfo obj2 = (CardInfo)obj;
        return obj2.Name.Equals(this.Name);
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

    public bool QuickEffect { get; set; } = false;

    public string EffectType { get; set; }
        
    [MaxLength(500)]
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
    public int SplashHashSum { get; set; }
    [ManyToOne]
    public CardInfoDB Card { get; set; }
}
