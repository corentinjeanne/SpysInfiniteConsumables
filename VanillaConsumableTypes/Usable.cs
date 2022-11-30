using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using SPIC.ConsumableGroup;
using SPIC.Config;

namespace SPIC.VanillaConsumableTypes; 

public enum UsableCategory : byte {
    None = Category.None,

    Weapon,
    Recovery,
    Buff, // red potion
    PlayerBooster,
    WorldBooster,

    Summoner, // power cell, truffle worm, ethernia crystal, torch god
    Critter,
    Explosive,
    Tool, //confetti and cannon ammo, tree globes

    Unknown = Category.Unknown
}

public class UsableRequirements {
    [Label("$Mods.SPIC.Groups.Usable.weapons")]
    public ItemCountWrapper Weapons = new(2.0f);
    [Label("$Mods.SPIC.Groups.Usable.potions")]
    public ItemCountWrapper Potions = new(1.0f, 30);
    [Label("$Mods.SPIC.Groups.Usable.boosters")]
    public ItemCountWrapper Boosters = new(5, 20);
    [Label("$Mods.SPIC.Groups.Usable.summoners")]
    public ItemCountWrapper Summoners = new(3, 20);
    [Label("$Mods.SPIC.Groups.Usable.critters")]
    public ItemCountWrapper Critters = new(10, 99);
    [Label("$Mods.SPIC.Groups.Usable.tools")]
    public ItemCountWrapper Tools = new(1.0f);
}


public class Usable : ItemGroup<Usable, UsableCategory>, IConfigurable<UsableRequirements>, IDetectable {

    public override Mod Mod => SpysInfiniteConsumables.Instance;
    public override int IconType => ItemID.EndlessMusketPouch;

#nullable disable
    public UsableRequirements Settings { get; set; }
#nullable restore
    
    public override IRequirement Requirement(UsableCategory category) {
        return category switch {
            UsableCategory.Weapon => new CountRequirement((ItemCount)Settings.Weapons),
            UsableCategory.Recovery => new CountRequirement(new ItemCount(Settings.Potions){MaxStack = 99}),
            UsableCategory.Buff => new CountRequirement((ItemCount)Settings.Potions),
            UsableCategory.PlayerBooster or UsableCategory.WorldBooster => new CountRequirement((ItemCount)Settings.Boosters),

            UsableCategory.Summoner => new CountRequirement((ItemCount)Settings.Summoners),
            UsableCategory.Critter => new CountRequirement((ItemCount)Settings.Critters),
            UsableCategory.Explosive => new CountRequirement(new ItemCount(Settings.Tools){MaxStack=99}),
            UsableCategory.Tool or UsableCategory.Unknown => new CountRequirement((ItemCount)Settings.Tools),

            UsableCategory.None or _ => new NoRequirement(),
        };
    }

    public override UsableCategory GetCategory(Item item) {

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

    public override Microsoft.Xna.Framework.Color DefaultColor => Colors.RarityCyan;
    public override TooltipLine TooltipLine => TooltipHelper.AddedLine("Consumable", Lang.tip[35].Value);
}