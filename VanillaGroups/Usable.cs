using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using SPIC.Configs;
using Terraria.Localization;

namespace SPIC.VanillaGroups; 

public enum UsableCategory : byte {
    None = CategoryHelper.None,

    Weapon,
    Recovery,
    Buff,
    PlayerBooster,
    WorldBooster,

    Summoner,
    Critter,
    Explosive,
    Tool, // TODO Fargo's summons

    Unknown = CategoryHelper.Unknown
}

public class UsableRequirements {
    [Label($"${Localization.Keys.Groups}.Usable.Weapons")]
    public ItemCountWrapper Weapons = new(){Stacks=2};
    [Label($"${Localization.Keys.Groups}.Usable.Potions")]
    public ItemCountWrapper Potions = new(30){Stacks=1};
    [Label($"${Localization.Keys.Groups}.Usable.Boosters")]
    public ItemCountWrapper Boosters = new(20){Items=5};
    [Label($"${Localization.Keys.Groups}.Usable.Summoners")]
    public ItemCountWrapper Summoners = new(20){Items=3};
    [Label($"${Localization.Keys.Groups}.Usable.Critters")]
    public ItemCountWrapper Critters = new(99){Items=10};
    [Label($"${Localization.Keys.Groups}.Usable.Tools")]
    public ItemCountWrapper Tools = new(){Stacks=1};
}


public class Usable : ItemGroup<Usable, UsableCategory>, IConfigurable<UsableRequirements>, IDetectable {
    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override string Name => Language.GetTextValue($"{Localization.Keys.Groups}.Usable.Name");
    public override int IconType => ItemID.EndlessMusketPouch;

    public override Requirement<ItemCount> Requirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => new CountRequirement<ItemCount>(this.Settings().Weapons),
            UsableCategory.Recovery => new CountRequirement<ItemCount>(new(this.Settings().Potions){MaxStack = 99}),
            UsableCategory.Buff => new CountRequirement<ItemCount>(this.Settings().Potions),
            UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new CountRequirement<ItemCount>(this.Settings().Boosters),

            UsableCategory.Summoner => new CountRequirement<ItemCount>(this.Settings().Summoners),
            UsableCategory.Critter => new CountRequirement<ItemCount>(this.Settings().Critters),
            UsableCategory.Explosive => new CountRequirement<ItemCount>(new(this.Settings().Tools){MaxStack=99}),
            UsableCategory.Tool or UsableCategory.Unknown => new CountRequirement<ItemCount>(this.Settings().Tools),

            UsableCategory.None or _ => new NoRequirement<ItemCount>(),
        };
    }

    public override UsableCategory GetCategory(Item item) {

        // Vanilla inconsitancies or special items
        switch (item.type) {
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

        // Most modded summoners, booster and non buff potions, modded liquids...
        return UsableCategory.Unknown;
    }

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityCyan;
    public override TooltipLine TooltipLine => new(Mod, "Consumable", Lang.tip[35].Value);

    public bool IncludeUnknown => true;
}