using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum UsableCategory : byte {
    None = ConsumableType.NoCategory,

    Weapon,
    Recovery,
    Buff,
    PlayerBooster,
    WorldBooster,

    Summoner,
    Critter,
    Explosive,
    Tool,

    Unknown = ConsumableType.UnknownCategory
}

public class UsableRequirements {
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Weapons")]
    public int Weapons = -2;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Potions")]
    public int Potions = -1;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Boosters")]
    public int Boosters = 5;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Summoners")]
    public int Summoners = 3;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Critters")]
    public int Critters = 10;
    [Range(-50, 999), Label("$Mods.SPIC.Configs.Requirements.Requirements.Tools")]
    public int Tools = -1;
}


public class Usable : ConsumableType<Usable> {

    public override bool CategoryDetection => true;

    public override int MaxStack(byte category) => (UsableCategory)category switch {
        UsableCategory.Weapon => 999,
        UsableCategory.Recovery => 99,
        UsableCategory.Buff => 30,

        UsableCategory.PlayerBooster => 99,
        UsableCategory.WorldBooster => 20,

        UsableCategory.Summoner => 20,
        UsableCategory.Critter => 99,
        UsableCategory.Explosive => 99,
        UsableCategory.Tool or UsableCategory.Unknown => 999,

        UsableCategory.None or _ => 999,
    };
    public override int Requirement(byte category) {
        UsableRequirements reqs = (UsableRequirements)Requirements;
        return (UsableCategory)category switch {
            UsableCategory.Weapon => reqs.Weapons,
            UsableCategory.Recovery or UsableCategory.Buff => reqs.Potions,
            UsableCategory.PlayerBooster or UsableCategory.WorldBooster => reqs.Boosters,

            UsableCategory.Summoner => reqs.Summoners,
            UsableCategory.Critter => reqs.Critters,
            UsableCategory.Tool or UsableCategory.Explosive or UsableCategory.Unknown => reqs.Tools,

            UsableCategory.None or _ => NoRequirement,
        };
    }

    public override byte GetCategory(Item item) {

        if (!item.consumable || item.Placeable()) return (byte)UsableCategory.None;

        if (item.bait != 0) return (byte)UsableCategory.Critter;

        if (item.useStyle == ItemUseStyleID.None) return (byte)UsableCategory.None;

        // Vanilla inconsitancies or special items
        switch (item.type) {
        case ItemID.FallenStar: return (byte)UsableCategory.None;
        case ItemID.PirateMap or ItemID.EmpressButterfly: return (byte)UsableCategory.Summoner;
        case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return (byte)UsableCategory.Critter;
        case ItemID.CombatBook: return (byte)UsableCategory.WorldBooster;
        }

        if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
            return (byte)UsableCategory.Summoner;

        if (item.makeNPC != NPCID.None) return (byte)UsableCategory.Critter;

        if (item.damage > 0) return (byte)UsableCategory.Weapon;

        if (item.buffType != 0 && item.buffTime != 0) return (byte)UsableCategory.Buff;
        if (item.healLife > 0 || item.healMana > 0 || item.potion) return (byte)UsableCategory.Recovery;

        if (item.shoot != ProjectileID.None) return (byte)UsableCategory.Tool;

        if (item.hairDye != -1) return (byte)UsableCategory.PlayerBooster;

        // Most modded summoners, booster and non buff potions, modded liquids...
        return (byte)UsableCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor() => new(0, 255, 200);
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Consumable", Lang.tip[35].Value);
    public override string CategoryKey(byte category) => (UsableCategory)category == UsableCategory.Unknown ? $"Mods.SPIC.Categories.Unknown" : $"Mods.SPIC.Categories.Usable.{(UsableCategory)category}";

    public override UsableRequirements CreateRequirements() => new();

    public override byte[] HiddenCategories => new[] { NoCategory };
}