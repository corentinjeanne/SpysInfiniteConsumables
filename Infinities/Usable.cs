using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SPIC.Infinities;

public enum UsableCategory {
    None,

    Weapon,
    Recovery,
    Buff,
    PlayerBooster,
    WorldBooster,

    Summoner,
    Critter,
    Explosive,
    Tool, // TODO Fargo's summons

    Unknown
}

public class UsableRequirements {
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Weapons")]
    public Count Weapons = 2 * 999;
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Potions")]
    public Count Potions = 30;
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Boosters")]
    public Count Boosters = 5;
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Summoners")]
    public Count Summoners = 3;
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Critters")]
    public Count Critters = 10;
    [LabelKey($"${Localization.Keys.Infinties}.Usable.Tools")]
    public Count Tools = 99;
}


public class Usable : InfinityStatic<Usable, Items, Item, UsableCategory> {

    public override int IconType => ItemID.EndlessMusketPouch;
    public override Color DefaultColor => Colors.RarityCyan;

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();
        Config = InfinityManager.RegisterConfig<UsableRequirements>(this);
    }

    public override Requirement GetRequirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => new(Config.Value.Weapons),
            UsableCategory.Recovery => new(Config.Value.Potions),
            UsableCategory.Buff => new(Config.Value.Potions),
            UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new(Config.Value.Boosters),

            UsableCategory.Summoner => new(Config.Value.Summoners),
            UsableCategory.Critter => new(Config.Value.Critters),
            UsableCategory.Explosive => new(Config.Value.Tools),
            UsableCategory.Tool or UsableCategory.Unknown => new(Config.Value.Tools),

            UsableCategory.None or _ => new(),
        };
    }

    public override UsableCategory GetCategory(Item item) {

        // Vanilla inconsitancies or special items
        switch (item.type) {
        case ItemID.Geode: return UsableCategory.None; // Grabbag
        case ItemID.FallenStar: return UsableCategory.None; // usable
        case ItemID.PirateMap or ItemID.EmpressButterfly: return UsableCategory.Summoner; // sorting priority error
        case ItemID.LihzahrdPowerCell or ItemID.DD2ElderCrystal: return UsableCategory.Summoner; // ItemUseStyleID.None
        case ItemID.RedPotion: return UsableCategory.Buff;
        case ItemID.TreeGlobe or ItemID.WorldGlobe: return UsableCategory.WorldBooster;
        }

        if (!item.consumable || item.Placeable()) return UsableCategory.None;


        if (item.bait == 0 && item.useStyle == ItemUseStyleID.None) return UsableCategory.None;


        if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
            return UsableCategory.Summoner;

        if (item.bait != 0) return UsableCategory.Critter;
        if (item.makeNPC != NPCID.None) return UsableCategory.Critter;

        if (item.damage > 0) return UsableCategory.Weapon;

        if (item.buffType != 0 && item.buffTime != 0) return UsableCategory.Buff;
        if (item.healLife > 0 || item.healMana > 0 || item.potion) return UsableCategory.Recovery;

        if (item.shoot != ProjectileID.None) return UsableCategory.Tool;

        if (item.hairDye != -1) return UsableCategory.PlayerBooster;

        if (ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[item.type]) return UsableCategory.Explosive;

        // Most modded summoners, booster
        if (ItemID.Sets.SortingPriorityBossSpawns[item.type] > 0) return UsableCategory.Unknown;

        return item.chlorophyteExtractinatorConsumable ? UsableCategory.None : UsableCategory.Tool;
    }

    public Wrapper<UsableRequirements> Config = null!;

    public override Item DisplayedValue(Item consumable) => consumable.fishingPole <= 0 ? consumable : Main.LocalPlayer.PickBait() ?? consumable;

    public override (TooltipLine, TooltipLineID?) GetTooltipLine(Item item) {
        Item ammo = DisplayedValue(item);
        if (ammo == item) return (new(Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
        return (new(Mod, "PoleConsumes", Lang.tip[52].Value + ammo.Name), TooltipLineID.WandConsumes);

    }
}
