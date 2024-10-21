using Terraria;
using Terraria.ID;
using SPIC.Configs;
using SPIC.Default.Displays;
using Terraria.ModLoader;
using SpikysLib;
using System.Collections.Generic;
using SpikysLib.Configs.UI;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace SPIC.Default.Infinities;

public enum UsableCategory {
    None,

    Weapon,
    Potion,
    Booster,

    Summoner,
    Critter,
    Tool,

    Unknown
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
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

    public sealed override InfinityDefaults Defaults => new() { Color = new(136, 226, 255) };

    public override long GetRequirement(UsableCategory category) => category switch {
        UsableCategory.Weapon => Config.Weapon,
        UsableCategory.Potion => Config.Potion,
        UsableCategory.Booster => Config.Booster,
        UsableCategory.Summoner => Config.Summoner,
        UsableCategory.Critter => Config.Critter,
        UsableCategory.Tool or UsableCategory.Unknown => Config.Tool,
        _ => 0,
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

        if (item.shoot != ProjectileID.None) return UsableCategory.Tool;

        if (item.hairDye != -1) return UsableCategory.Booster;

        return item.chlorophyteExtractinatorConsumable ? UsableCategory.None : UsableCategory.Unknown; // Confetti, Recall like potion, shimmer boosters, boosters, modded summons
    }

    public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
        if (displayed == item.type) return (new(Instance.Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
        return (new(Instance.Mod, "PoleConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    }

    protected override void ModifyDisplayedConsumables(Item item, int context, ref List<Item> displayed) {
        Item? ammo = item.fishingPole > 0 ? Main.LocalPlayer.PickBait() : null;
        if (ammo is not null) displayed.Add(ammo);
    }

    protected override void ModifyDisplayedInfinity(Item item, int context, Item consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (context == ItemSlot.Context.InventoryAmmo && InfinityManager.GetCategory(item, this) == UsableCategory.Critter) visibility = InfinityVisibility.Exclusive;
    }
}
