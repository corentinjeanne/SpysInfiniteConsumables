using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Infinities;
public enum ConsumableCategory : byte {
    None = Infinity.NoCategory,

    Weapon,
    Recovery,
    Buff,
    PlayerBooster,
    WorldBooster,

    Summoner,
    Critter,
    Explosive,
    Tool,

    Unknown = Infinity.UnknownCategory
}

public class Consumable : Infinity<Consumable> {

    public override int MaxStack(byte category) => (ConsumableCategory)category switch {
        ConsumableCategory.Weapon => 999,
        ConsumableCategory.Recovery => 99,
        ConsumableCategory.Buff => 30,

        ConsumableCategory.PlayerBooster => 99,
        ConsumableCategory.WorldBooster => 20,

        ConsumableCategory.Summoner => 20,
        ConsumableCategory.Critter => 99,
        ConsumableCategory.Explosive => 99,
        ConsumableCategory.Tool or ConsumableCategory.Unknown => 999,

        ConsumableCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        Configs.Requirements c = Configs.Requirements.Instance;
        return (ConsumableCategory)category switch {
            ConsumableCategory.Weapon => c.consumables_Weapons,
            ConsumableCategory.Recovery or ConsumableCategory.Buff => c.consumables_Potions,
            ConsumableCategory.PlayerBooster or ConsumableCategory.WorldBooster => c.consumables_Boosters,

            ConsumableCategory.Summoner => c.consumables_Summoners,
            ConsumableCategory.Critter => c.consumables_Critters,
            ConsumableCategory.Tool or ConsumableCategory.Explosive or ConsumableCategory.Unknown => c.consumables_Tools,

            ConsumableCategory.None or _ => Infinity.NoRequirement,
        };
    }

    public override bool Enabled => Configs.Requirements.Instance.InfiniteConsumables;

    public override byte GetCategory(Item item) {

        // var categories = Configs.Requirements.Instance.GetCustomCategories(item.type);
        // if (categories.Consumable.HasValue) return (byte)categories.Consumable.Value;

        // TODO improve detection config and API
        var autos = Configs.CategoryDetection.Instance.GetDetectedCategories(item.type);
        if (autos.Consumable.HasValue) return (byte)autos.Consumable.Value;

        if (!item.consumable || item.Placeable()) return (byte)ConsumableCategory.None;

        if (item.bait != 0) return (byte)ConsumableCategory.Critter;

        if (item.useStyle == ItemUseStyleID.None) return (byte)ConsumableCategory.None;

        // Vanilla inconsitancies or special items
        switch (item.type) {
        case ItemID.FallenStar: return (byte)ConsumableCategory.None;
        case ItemID.PirateMap or ItemID.EmpressButterfly: return (byte)ConsumableCategory.Summoner;
        case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return (byte)ConsumableCategory.Critter;
        case ItemID.CombatBook: return (byte)ConsumableCategory.WorldBooster;
        }

        if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
            return (byte)ConsumableCategory.Summoner;

        if (item.makeNPC != NPCID.None) return (byte)ConsumableCategory.Critter;

        if (item.damage > 0) return (byte)ConsumableCategory.Weapon;

        if (item.buffType != 0 && item.buffTime != 0) return (byte)ConsumableCategory.Buff;
        if (item.healLife > 0 || item.healMana > 0 || item.potion) return (byte)ConsumableCategory.Recovery;

        if (item.shoot != ProjectileID.None)
            return autos.Explosive ? (byte)ConsumableCategory.Explosive : (byte)ConsumableCategory.Tool;

        if (item.hairDye != -1) return (byte)ConsumableCategory.PlayerBooster;

        // Most modded summoners, booster and non buff potions, modded liquids...
        return (byte)ConsumableCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color Color => Configs.InfinityDisplay.Instance.color_Consumables;
    public override TooltipLine TooltipLine => AddedLine("Consumable", Lang.tip[35].Value);
    public override string CategoryKey(byte category) => (ConsumableCategory)category == ConsumableCategory.Unknown ? $"Mods.SPIC.Categories.Unknown" : $"Mods.SPIC.Categories.Consumable.{(ConsumableCategory)category}";
    public override byte[] HiddenCategories => new[] { NoCategory };
}