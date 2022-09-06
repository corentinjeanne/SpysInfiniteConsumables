using System.Collections.Generic;
using Terraria;

using Terraria.Localization;
using Terraria.ModLoader;

namespace SPIC.ConsumableTypes;

public enum InfinityDisplayLevel {
    None,
    Category,
    Requirement,
    Infinity,
    All
}

public abstract class ConsumableType<Type> : ConsumableType where Type : ConsumableType<Type>, new() {
    public static readonly Type Instance = new();
    public static int ID => Instance.UID;
    protected ConsumableType(){}
    static ConsumableType(){}

    public static void Register(int infinityID) => Instance.UID = InfinityManager.RegisterConsumableType(Instance, infinityID);
}

public abstract class ConsumableType {
    public int UID { get; protected set; } = -1;

    public virtual string Name => GetType().Name;

    public abstract int MaxStack(byte category);
    public abstract int Requirement(byte category);
    public object Requirements { get; internal set; }
    public abstract object CreateRequirements();

    // public virtual Item ItemFromType(int type) => new(type);
    // public virtual int Type(Item item) => item.type;

    // public virtual long CountItems(Player player, int type) => player.CountItems(type, true);
    // public virtual long CountItems(Player player, Item item) => CountItems(player, Type(item));
    public virtual long CountItems(Player player, Item item) => player.CountItems(item.type, true);


    public virtual bool Customs => true;
    public virtual bool CategoryDetection => false;

    public virtual bool ConsumesAmmo(Item item) => false;
    public virtual Item GetAmmo(Player player, Item weapon) => null;
    public bool HasAmmo(Player player, Item weapon, out Item ammo) => (ammo = GetAmmo(player, weapon)) != null;

    // public virtual byte GetCategory(int type) => GetCategory(ItemFromType(type));
    public abstract byte GetCategory(Item item);

    // public virtual int GetRequirement(int type) => GetRequirement(ItemFromType(type));
    public virtual int GetRequirement(Item item) => Requirement(item.GetCategory(UID));

    // public long GetInfinity(Player player, int type) => GetInfinity(type, CountItems(player, type));
    public long GetInfinity(Player player, Item item) => GetInfinity(item, CountItems(player, item));

    // public virtual long GetInfinity(int type, long count) => GetInfinity(ItemFromType(type), count);
    public virtual long GetInfinity(Item item, long count)
        => InfinityManager.CalculateInfinity(
            (int)System.MathF.Min(Globals.ConsumptionItem.MaxStack(item.type), MaxStack(item.GetCategory(UID))),
            count,
            item.GetRequirement(UID),
            1
        );


    public virtual bool IsFullyInfinite(Item item, long infinity) => true;
    public virtual KeyValuePair<int, long>[] GetPartialInfinity(Item item, long infinity) => new[] { new KeyValuePair<int, long>(item.type, infinity) };

    public abstract Microsoft.Xna.Framework.Color DefaultColor();

    public abstract TooltipLine TooltipLine { get; }
    public virtual string MissingLinePosition => null;
    public virtual TooltipLine AmmoLine(Item weapon, Item ammo) => TooltipHelper.AddedLine(Name + "Consumes", Language.GetTextValue($"Mods.SPIC.ItemTooltip.weaponAmmo", ammo.Name));

    public abstract string CategoryKey(byte category);
    public virtual byte[] HiddenCategories => new[] { NoCategory, UnknownCategory };

    public virtual InfinityDisplayLevel GetInfinityDisplayLevel(Item item, bool isACopy){
        Player player = Main.LocalPlayer;
        if(item.playerIndexTheItemIsReservedFor != Main.myPlayer) return InfinityDisplayLevel.None;
        if (isACopy) {
            static bool AreSimilarItems(Item a, Item b) => a.type == b.type && a.stack == b.stack;
            if(AreSimilarItems(Main.mouseItem, item)
                    || System.Array.Find(player.inventory, i => AreSimilarItems(i, item)) is not null
                    || (player.InChest(out var chest) && System.Array.Find(chest, i => AreSimilarItems(i, item)) is not null)
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
                return InfinityDisplayLevel.All;
        } else {
            if(Main.mouseItem == item
                    || System.Array.IndexOf(player.inventory, item) != -1
                    || (player.InChest(out Item[] chest) && System.Array.IndexOf(chest, item) != -1)
                    || (SpysInfiniteConsumables.MagicStorageLoaded && CrossMod.MagicStorageIntegration.Countains(item)))
                return InfinityDisplayLevel.All;
        }
        return InfinityDisplayLevel.None;
    }

    public const byte NoCategory = 0;
    public const byte UnknownCategory = 255;

    public const int NoRequirement = 0;

    public const long NoInfinity = -2L;
    public const long NotInfinite = -1L;
}