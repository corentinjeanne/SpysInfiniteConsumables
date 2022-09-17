using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public enum InfinityDisplayFlag : byte {
    None = 0b0000,
    Category = 0b0001,
    Requirement = 0b0010,
    Infinity = 0b0100,
    All = Category | Requirement | Infinity
}

public interface IConsumableType {
    byte GetCategory(Item item);
    int GetRequirement(Item item);
    long GetInfinity(Player player, Item item);
}


public abstract class ConsumableType<Type> : ConsumableType where Type : ConsumableType<Type>, new() {
    public static readonly Type Instance = new();
    public static int ID => Instance.UID;
    protected ConsumableType(){}
    static ConsumableType(){}

    public static void Register(int infinityID) => InfinityManager.RegisterConsumableType(Instance, infinityID);
}

public abstract class ConsumableType {
    public int UID { get; internal set; }
    public abstract Mod Mod { get; }
    public virtual string Name => GetType().Name;

    public virtual string LocalizedName => Name;

    public abstract int MaxStack(byte category);
    public abstract int Requirement(byte category);

    public object ConfigRequirements { get; internal set; }
    public abstract object CreateRequirements();

    public virtual long CountItems(Player player, Item item) => player.CountItems(item.type, true);

    // public virtual byte GetCategory(int type) => GetCategory(ItemFromType(type));
    public abstract byte GetCategory(Item item);

    // public virtual int GetRequirement(int type) => GetRequirement(ItemFromType(type));
    public virtual int GetRequirement(Item item) => Requirement(item.GetCategory(UID));

    // public long GetInfinity(Player player, int type) => GetInfinity(type, CountItems(player, type));
    public virtual long GetInfinity(Player player, Item item) => GetInfinity(item, CountItems(player, item));

    // public virtual long GetInfinity(int type, long count) => GetInfinity(ItemFromType(type), count);
    public virtual long GetInfinity(Item item, long count)
        => InfinityManager.CalculateInfinity(
            (int)System.MathF.Min(Globals.ConsumptionItem.MaxStack(item.type), MaxStack(item.GetCategory(UID))),
            count,
            item.GetRequirement(UID),
            1
        );

    public abstract Microsoft.Xna.Framework.Color DefaultColor();

    public abstract TooltipLine TooltipLine { get; }
    public virtual string MissingLinePosition => null;

    public abstract string LocalizedCategoryName(byte category);
    public virtual byte[] HiddenCategories => new[] { NoCategory, UnknownCategory };

    public virtual InfinityDisplayFlag GetInfinityDisplayLevel(Item item, bool isACopy){
        Player player = Main.LocalPlayer;
        bool AreSameItems(Item a, Item b) => isACopy ? (a.type == b.type && a.stack == b.stack) : a == b;

        if (item.playerIndexTheItemIsReservedFor != Main.myPlayer) return InfinityDisplayFlag.All & ~InfinityDisplayFlag.Infinity;
        if(System.Array.Find(player.armor, i => AreSameItems(i, item)) is not null
                || System.Array.Find(player.miscEquips, i => AreSameItems(i, item)) is not null)
            return InfinityDisplayFlag.None;

        if(AreSameItems(Main.mouseItem, item)
                || System.Array.Find(player.inventory, i => AreSameItems(i, item)) is not null
                || (player.InChest(out var chest) && System.Array.Find(chest, i => AreSameItems(i, item)) is not null)
                || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
            return InfinityDisplayFlag.All;
        
        return InfinityDisplayFlag.All & ~InfinityDisplayFlag.Infinity;
    }

    public override string ToString() => Name;

    public const byte NoCategory = 0;
    public const byte UnknownCategory = 255;

    public const int NoRequirement = 0;

    public const long NoInfinity = -2L;
    public const long NotInfinite = -1L;
    public const long MinInfinity = 0L;

}

// TODO add more interface
// public interface IConfigurable {}
// public interface IConfigurable<T> : IConfigurable {}
// public interface IDrawable {}

public interface ICustomizable {}
public interface IDetectable {}

public interface IAmmunition {
    public bool ConsumesAmmo(Item item);
    public Item GetAmmo(Player player, Item weapon);
    public bool HasAmmo(Player player, Item weapon, out Item ammo) => (ammo = GetAmmo(player, weapon)) != null;

    public TooltipLine AmmoLine(Item weapon, Item ammo); //  => TooltipHelper.AddedLine(Name + "Consumes", Language.GetTextValue($"Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name))
}

public interface IPartialInfinity {
    public long GetFullInfinity(Player player, Item item);
    // ? Remove
    public virtual KeyValuePair<int, long>[] GetPartialInfinity(Item item, long infinity) => new KeyValuePair<int, long>[] { new(item.type, infinity) };
}