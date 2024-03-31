using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

using SPIC.Configs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Collections.Generic;
using SpikysLib;
using SpikysLib.Extensions;
using SpikysLib.Configs.UI;
using SPIC.Default.Displays;

namespace SPIC.Default.Infinities;

public enum UsableCategory {
    None,

    Weapon,
    Potion,
    // Recovery,
    // Buff,
    Booster,
    // PlayerBooster,
    // WorldBooster,

    Summoner, // TODO Fargo's summons
    Critter,
    // Explosive,
    Tool,

    Unknown
}

public sealed class UsableRequirements {
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Weapon")]
    public Count Weapon = 2 * 999;
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Potion")]
    public Count Potion = 30;
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Booster")]
    public Count Booster = 5;
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Summoner")]
    public Count Summoner = 3;
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Critter")]
    public Count Critter = 10;
    [LabelKey($"${Localization.Keys.Infinities}.Usable.Tool")]
    public Count Tool = 99;
}


public sealed class Usable : Infinity<Item, UsableCategory>, ITooltipLineDisplay {

    public override Group<Item> Group => Items.Instance;
    public static Usable Instance = null!;
    public static Wrapper<UsableRequirements> Config = null!;


    public override int IconType => ItemID.EndlessMusketPouch;
    public override Color Color { get; set; } = Colors.RarityCyan;

    public override void SetStaticDefaults() {
        Config = Group.AddConfig<UsableRequirements>(this);
    }

    public override Requirement GetRequirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => new(Config.Value.Weapon),
            UsableCategory.Potion => new(Config.Value.Potion),
            // UsableCategory.Recovery => new(Config.Value.Potion),
            // UsableCategory.Buff => new(Config.Value.Potion),
            UsableCategory.Booster => new(Config.Value.Booster),
            // UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new(Config.Value.Booster),
            UsableCategory.Summoner => new(Config.Value.Summoner),
            UsableCategory.Critter => new(Config.Value.Critter),
            // UsableCategory.Explosive => new(Config.Value.Tool),
            UsableCategory.Tool or UsableCategory.Unknown => new(Config.Value.Tool),
            _ => new(),
        };
    }

    public override UsableCategory GetCategory(Item item) {

        if (!item.consumable || item.Placeable()) return UsableCategory.None;

        // Vanilla inconsitancies or special items
        switch (item.type) {
        case ItemID.Geode: return UsableCategory.None; // Grabbag
        case ItemID.FallenStar: return UsableCategory.None; // usable
        case ItemID.PirateMap or ItemID.EmpressButterfly: return UsableCategory.Summoner; // sorting priority
        case ItemID.LihzahrdPowerCell or ItemID.DD2ElderCrystal: return UsableCategory.Summoner; // ItemUseStyleID.None
        case ItemID.RedPotion: return UsableCategory.Potion;
        case ItemID.TreeGlobe or ItemID.WorldGlobe: return UsableCategory.Booster;
        }

        if (item.bait == 0 && item.useStyle == ItemUseStyleID.None) return UsableCategory.None;


        if (0 < ItemID.Sets.SortingPriorityBossSpawns[item.type] && ItemID.Sets.SortingPriorityBossSpawns[item.type] <= 17 && item.type != ItemID.TreasureMap)
            return UsableCategory.Summoner;

        if (item.bait != 0 || item.makeNPC != NPCID.None) return UsableCategory.Critter;

        if (item.damage > 0) return UsableCategory.Weapon;

        if (item.buffType != 0 && item.buffTime != 0) return UsableCategory.Potion;
        if (item.healLife > 0 || item.healMana > 0 || item.potion) return UsableCategory.Potion;

        // if (ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[item.type]) return UsableCategory.Explosive;
        if (item.shoot != ProjectileID.None) return UsableCategory.Tool;

        if (item.hairDye != -1) return UsableCategory.Booster;

        return item.chlorophyteExtractinatorConsumable ? UsableCategory.None : UsableCategory.Unknown; // Confetti, Recall like potion, shimmer boosters, boosters, modded summons
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
        if (displayed == item.type) return (new(Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
        return (new(Mod, "PoleConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    }

    public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index >= 53 && 58 > index && InfinityManager.GetCategory(item, this) == UsableCategory.Critter) visibility = InfinityVisibility.Exclusive;
    }

    public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
        Item? item = consumable.fishingPole > 0 ? Main.LocalPlayer.PickBait() : null;
        if (item is not null) displayed.Add(item);
    }
}
