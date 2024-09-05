using Terraria;
using Terraria.ID;
using SPIC.Configs;
using Microsoft.Xna.Framework;

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


public sealed class Usable : Infinity<Item, UsableCategory>, IConfigurableComponents<UsableRequirements> {

    public override GroupInfinity<Item> Group => Consumable.Instance;
    public static Usable Instance = null!;

    public override Color DefaultColor => new(136, 226, 255, 255); // Stardust

    protected override Requirement GetRequirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => new(Configs.InfinitySettings.Get(this).Weapon),
            UsableCategory.Potion => new(Configs.InfinitySettings.Get(this).Potion),
            // UsableCategory.Recovery => new(Configs.Infinities.Get(this).Potion),
            // UsableCategory.Buff => new(Configs.Infinities.Get(this).Potion),
            UsableCategory.Booster => new(Configs.InfinitySettings.Get(this).Booster),
            // UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new(Configs.Infinities.Get(this).Booster),
            UsableCategory.Summoner => new(Configs.InfinitySettings.Get(this).Summoner),
            UsableCategory.Critter => new(Configs.InfinitySettings.Get(this).Critter),
            // UsableCategory.Explosive => new(Configs.Infinities.Get(this).Tool),
            UsableCategory.Tool or UsableCategory.Unknown => new(Configs.InfinitySettings.Get(this).Tool),
            _ => Requirement.None,
        };
    }

    protected override UsableCategory GetCategory(Item item) {

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

    // public (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int displayed) {
    //     if (displayed == item.type) return (new(Mod, "Consumable", Lang.tip[35].Value), TooltipLineID.Consumable);
    //     return (new(Mod, "PoleConsumes", Lang.tip[52].Value + Lang.GetItemName(displayed)), TooltipLineID.WandConsumes);
    // }

    // public override void ModifyDisplay(Player player, Item item, Item consumable, ref Requirement requirement, ref long count, List<object> extras, ref InfinityVisibility visibility) {
    //     int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
    //     if (index >= 53 && 58 > index && InfinityManager.GetCategory(item, this) == UsableCategory.Critter) visibility = InfinityVisibility.Exclusive;
    // }

    // public override void ModifyDisplayedConsumables(Item consumable, List<Item> displayed) {
    //     Item? item = consumable.fishingPole > 0 ? Main.LocalPlayer.PickBait() : null;
    //     if (item is not null) displayed.Add(item);
    // }
}
