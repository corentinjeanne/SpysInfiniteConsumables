using System.Collections.Generic;

using Terraria.ModLoader;

namespace SPIC {
    public static class TooltipHelper {
        public static readonly List<string> TooltipLinesOrder = new(){
            "ItemName",
            "Favorite",
            "FavoriteDesc",
            "NoTransfer",
            "Social",
            "SocialDesc",
            "Damage",
            "CritChance",
            "Speed",
            "Knockback",
            "FishingPower",
            "NeedsBait",
            "BaitPower",
            "Equipable",
            "WandConsumes",
            "Quest",
            "Vanity",
            "VanityLegal",
            "Defense",
            "PickPower",
            "AxePower",
            "HammerPower",
            "TileBoost",
            "HealLife",
            "HealMana",
            "UseMana",
            "Placeable",
            "Ammo",
            "Consumable",
            "Material",
            "Tooltip",
            "EtherianManaWarning",
            "WellFedExpert",
            "BuffTime",
            "OneDropLogo",
            "PrefixDamage",
            "PrefixSpeed",
            "PrefixCritChance",
            "PrefixUseMana",
            "PrefixSize",
            "PrefixShootSpeed",
            "PrefixKnockback",
            "PrefixAccDefense",
            "PrefixAccMaxMana",
            "PrefixAccCritChance",
            "PrefixAccDamage",
            "PrefixAccMoveSpeed",
            "PrefixAccMeleeSpeed",
            "SetBonus",
            "Expert",
            "Master",
            "JourneyResearch",
            "BestiaryNotes",
            "SpecialPrice",
            "Price"
        };

        internal static TooltipLine AddedLine(string name, string value) => new(SpysInfiniteConsumables.Instance, name, value) { OverrideColor = Terraria.ID.Colors.RarityTrash };
        
        public static TooltipLine? FindLine(this List<TooltipLine> tooltips, string name)
            => tooltips.Find(l => (l.Mod == "Terraria" || l.Mod == nameof(SpysInfiniteConsumables)) && l.Name == name);
        
        public static TooltipLine AddLine(this List<TooltipLine> tooltips, string after, TooltipLine line) {
            int addIndex = TooltipLinesOrder.FindIndex(n => n == after);
            for (int i = 0; i < tooltips.Count; i++) {
                if(tooltips[i].Name == line.Name) return tooltips[i];
                int lookingAt = tooltips[i].Name.StartsWith("Tooltip") ? TooltipLinesOrder.FindIndex(l => l == "Tooltip") : TooltipLinesOrder.FindIndex(l => l == tooltips[i].Name);
                if (lookingAt <= addIndex) continue;
                tooltips.Insert(i, line);
                return line;
            }
            tooltips.Add(line);
            return line;
        }

        public static TooltipLine FindorAddLine(this List<TooltipLine> tooltips, TooltipLine line, string? after, out bool addedLine) {
            TooltipLine? target = tooltips.FindLine(line.Name);
            if (addedLine = target is null)
                target = tooltips.AddLine(after ?? line.Name, line);
            return target!;
        }
        public static TooltipLine FindorAddLine(this List<TooltipLine> tooltips, TooltipLine line, string? after = null) => FindorAddLine(tooltips, line, after, out _);
    }
}