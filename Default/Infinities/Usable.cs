using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using SPIC.Components;
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


public sealed class Usable : Infinity<Item>, IConfigurableComponents<UsableRequirements> {
    public static Customs<Item, UsableCategory> Customs = new(i => new(i.type));
    public static Group<Item> Group = new(() => ConsumableItem.InfinityGroup);
    public static Category<Item, UsableCategory> Category = new(GetRequirement, GetCategory);
    public static Usable Instance = null!;
    public static TooltipDisplay TooltipDisplay = new(GetTooltipLine);

    public override Color DefaultColor => new(136, 226, 255, 255); // Stardust

    private static Optional<Requirement> GetRequirement(UsableCategory category) => category switch {
        UsableCategory.Weapon => new(InfinitySettings.Get(Instance).Weapon),
        UsableCategory.Potion => new(InfinitySettings.Get(Instance).Potion),
        // UsableCategory.Recovery => new(Infinities.Get(Instance).Potion),
        // UsableCategory.Buff => new(Infinities.Get(Instance).Potion),
        UsableCategory.Booster => new(InfinitySettings.Get(Instance).Booster),
        // UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new(Infinities.Get(Instance).Booster),
        UsableCategory.Summoner => new(InfinitySettings.Get(Instance).Summoner),
        UsableCategory.Critter => new(InfinitySettings.Get(Instance).Critter),
        // UsableCategory.Explosive => new(Infinities.Get(Instance).Tool),
        UsableCategory.Tool or UsableCategory.Unknown => new(InfinitySettings.Get(Instance).Tool),
        _ => Requirement.None,
    };

    private static Optional<UsableCategory> GetCategory(Item item) {

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

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
        if (displayed == item.type) return (new(Instance.Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
        return (new(Instance.Mod, "PoleConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    }

    protected override Optional<InfinityVisibility> GetVisibility(Item item) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        return InventorySlots.Ammo.Contains(index) && InfinityManager.GetCategory(item, Category) == UsableCategory.Critter ? new(InfinityVisibility.Exclusive) : default;
    }

    protected override void ModifyDisplayedConsumables(Item item, ref List<Item> displayed) {
        Item? ammo = item.fishingPole > 0 ? Main.LocalPlayer.PickBait() : null;
        if (ammo is not null) displayed.Add(ammo);
    }
}
