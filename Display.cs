using System.ComponentModel;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC;

public abstract class Display : ModType, ILocalizedModType {

    [DefaultValue(true)]
    public virtual bool Enabled { get; internal set; } = true;
    public abstract int IconType { get; }

    public string LocalizationCategory => "Displays";
    public virtual LocalizedText DisplayName => this.GetLocalization("DisplayName", new System.Func<string>(PrettyPrintName));

    protected sealed override void Register() {
        ModTypeLookup<Display>.Register(this);
        DisplayLoader.Register(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
}

public abstract class DisplayStatic<TDisplay> : Display where TDisplay : DisplayStatic<TDisplay> {
    public override void SetStaticDefaults() => Instance = (TDisplay)this;
    public override void Unload() {
        Instance = null!;
        base.Unload();
    }

    public static TDisplay Instance { get; private set; } = null!;
}