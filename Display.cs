using Terraria.ModLoader;

namespace SPIC;

public abstract class Display : ModType {
    public virtual bool Enabled { get; internal set; } = true; // TODO default value

    protected sealed override void Register() {
        ModTypeLookup<Display>.Register(this);
        DisplayLoader.Register(this);
    }

    public sealed override void SetupContent() => SetStaticDefaults();
}

public class DisplayStatic<TDisplay> : Display where TDisplay : DisplayStatic<TDisplay> {
    public override void SetStaticDefaults() => Instance = (TDisplay)this;
    public override void Unload() {
        Instance = null!;
        base.Unload();
    }

    public static TDisplay Instance { get; private set; } = null!;
}