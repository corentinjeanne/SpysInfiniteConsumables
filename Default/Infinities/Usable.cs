using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Default.Displays;
using Terraria.ModLoader;
using SpikysLib;
using SpikysLib.Constants;
using System.Collections.Generic;

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
    public Count<UsableCategory> Weapon = 2 * 999;
    public Count<UsableCategory> Potion = 30;
    public Count<UsableCategory> Booster = 5;
    public Count<UsableCategory> Summoner = 3;
    public Count<UsableCategory> Critter = 10;
    public Count<UsableCategory> Tool = 99;
}


public sealed class Usable : Infinity<Item, UsableCategory>, IConfigProvider<UsableRequirements>, ITooltipLineDisplay {
    public static Usable Instance = null!;
    public UsableRequirements Config { get; set; } = null!;
    public override ConsumableInfinity<Item> Consumable => ConsumableItem.Instance;

    public override Color DefaultColor => new(136, 226, 255, 255); // Stardust

    public override Requirement GetRequirement(UsableCategory category) => category switch {
        UsableCategory.Weapon => new(Config.Weapon),
        UsableCategory.Potion => new(Config.Potion),
        // UsableCategory.Recovery => new(Infinities.Get(Instance).Potion),
        // UsableCategory.Buff => new(Infinities.Get(Instance).Potion),
        UsableCategory.Booster => new(Config.Booster),
        // UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new(Infinities.Get(Instance).Booster),
        UsableCategory.Summoner => new(Config.Summoner),
        UsableCategory.Critter => new(Config.Critter),
        // UsableCategory.Explosive => new(Infinities.Get(Instance).Tool),
        UsableCategory.Tool or UsableCategory.Unknown => new(Config.Tool),
        _ => default,
    };

    protected override UsableCategory GetCategoryInner(Item item) {

        if (!item.consumable || item.Placeable()) return UsableCategory.None;

        // Vanilla inconsistencies or special items
        switch (item.type) {
        case ItemID.Geode: return UsableCategory.None; // Grab bag
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
        if (displayed == item.type) return (new(Instance.Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
        return (new(Instance.Mod, "PoleConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    }

    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? ammo = item.fishingPole > 0 ? Main.LocalPlayer.PickBait() : null;
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override void ModifyDisplayedInfinity(Item item, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (index >= 53 && 58 > index && InfinityManager.GetCategory(item, this) == UsableCategory.Critter) visibility = InfinityVisibility.Exclusive;
    }
}
