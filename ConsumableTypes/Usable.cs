using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableTypes;
public enum UsableCategory : byte {
    None = IConsumableType.NoCategory,

    Weapon,
    Recovery,
    Buff,
    PlayerBooster,
    WorldBooster,

    Summoner,
    Critter,
    Explosive,
    Tool,

    Unknown = IConsumableType.UnknownCategory
}

public class UsableRequirements {
    [Label("$Mods.SPIC.Types.Usable.weapons")]
    public Configs.Requirement Weapons = -2;
    [Label("$Mods.SPIC.Types.Usable.potions")]
    public Configs.Requirement Potions = -1;
    [Label("$Mods.SPIC.Types.Usable.boosters")]
    public Configs.Requirement Boosters = 5;
    [Label("$Mods.SPIC.Types.Usable.summoners")]
    public Configs.Requirement Summoners = 3;
    [Label("$Mods.SPIC.Types.Usable.critters")]
    public Configs.Requirement Critters = 10;
    [Label("$Mods.SPIC.Types.Usable.tools")]
    public Configs.Requirement Tools = -1;
}


public class Usable : ConsumableType<Usable>, IStandardConsumableType<UsableCategory, UsableRequirements>, IDetectable, ICustomizable {

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.EndlessMusketPouch;

    public bool DefaultsToOn => true;
    public UsableRequirements Settings { get; set; }

    public int MaxStack(UsableCategory category) => category switch {
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
    public int Requirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => Settings.Weapons,
            UsableCategory.Recovery or UsableCategory.Buff => Settings.Potions,
            UsableCategory.PlayerBooster or UsableCategory.WorldBooster => Settings.Boosters,

            UsableCategory.Summoner => Settings.Summoners,
            UsableCategory.Critter => Settings.Critters,
            UsableCategory.Tool or UsableCategory.Explosive or UsableCategory.Unknown => Settings.Tools,

            UsableCategory.None or _ => IConsumableType.NoRequirement,
        };
    }

    public UsableCategory GetCategory(Item item) {

        if (!item.consumable || item.Placeable()) return UsableCategory.None;

        if (item.bait != 0) return UsableCategory.Critter;

        if (item.useStyle == ItemUseStyleID.None) return UsableCategory.None;

        // Vanilla inconsitancies or special items
        switch (item.type) {
        case ItemID.FallenStar: return UsableCategory.None;
        case ItemID.PirateMap or ItemID.EmpressButterfly: return UsableCategory.Summoner;
        case ItemID.LicenseBunny or ItemID.LicenseCat or ItemID.LicenseDog: return UsableCategory.Critter;
        case ItemID.CombatBook: return UsableCategory.WorldBooster;
        }

        if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
            return UsableCategory.Summoner;

        if (item.makeNPC != NPCID.None) return UsableCategory.Critter;

        if (item.damage > 0) return UsableCategory.Weapon;

        if (item.buffType != 0 && item.buffTime != 0) return UsableCategory.Buff;
        if (item.healLife > 0 || item.healMana > 0 || item.potion) return UsableCategory.Recovery;

        if (item.shoot != ProjectileID.None) return UsableCategory.Tool;

        if (item.hairDye != -1) return UsableCategory.PlayerBooster;

        // Most modded summoners, booster and non buff potions, modded liquids...
        return UsableCategory.Unknown;
    }

    public Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityCyan;
    public TooltipLine TooltipLine => TooltipHelper.AddedLine("Consumable", Lang.tip[35].Value);
}