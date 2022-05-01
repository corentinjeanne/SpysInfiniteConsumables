using Terraria.ModLoader;
using System.Collections.Generic;

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

		public static TooltipLine NewLine(Mod mod, string name, string text, Microsoft.Xna.Framework.Color? overrideColor = null) {
            TooltipLine line = new(mod, name, text){
				OverrideColor = overrideColor
			};
            return line;
		}
		public static TooltipLine FindLine(this List<TooltipLine> tooltips, string name) {
			return tooltips.Find(l => (l.Mod == "Terraria" || l.Mod == nameof(SpysInfiniteConsumables)) && l.Name == name);
		}
		public static TooltipLine AddLine(this List<TooltipLine> tooltips, TooltipLine line, string after) {
			int addIndex = TooltipLinesOrder.FindIndex(n => n == after);
			for (int i = 0; i < tooltips.Count; i++) {
				int lookingAt = tooltips[i].Name.StartsWith("Tooltip") ? TooltipLinesOrder.FindIndex(l => l == "Tooltip") : TooltipLinesOrder.FindIndex(l => l == tooltips[i].Name);
				if (lookingAt <= addIndex) continue;
				tooltips.Insert(i, line);
				return line;
			}
			tooltips.Add(line);
			return line;

		}
		public static TooltipLine FindorAddLine(this List<TooltipLine> tooltips, string name, TooltipLine notFound = null) {
			TooltipLine target = tooltips.FindLine(name);
			if (target == null) tooltips.AddLine(target = notFound, name);
			return target;
		}
	}
}
//localizedTextKey = id switch {
//	TooltipLineID.ItemName => "{0}",
//	TooltipLineID.Favorite => Lang.tip[56].Value,
//	TooltipLineID.FavoriteDesc => Lang.tip[57].Value,
//	TooltipLineID.NoTransfer => "$UI.ItemCannotBePlacedInsideItself",
//	TooltipLineID.Social => Lang.tip[0].Value,
//	TooltipLineID.SocialDesc => Lang.tip[1].Value,
//	TooltipLineID.Damage => "{0}" + Lang.tip[55].Value, // generic damage
//	TooltipLineID.CritChance => "{0}" + Lang.tip[5].Value,
//	TooltipLineID.Speed => Lang.tip[9].Value,
//	TooltipLineID.Knockback => Lang.tip[18].Value,
//	TooltipLineID.FishingPower => Language.GetTextValue("GameUI.PrecentFishingPower"),
//	TooltipLineID.NeedsBait => Language.GetTextValue("GameUI.BaitRequired"),
//	TooltipLineID.BaitPower => Language.GetTextValue("GameUI.BaitPower"),
//	TooltipLineID.Equipable => Lang.tip[23].Value,
//	TooltipLineID.WandConsumes => Lang.tip[52].Value,
//	TooltipLineID.Quest => Lang.inter[65].Value,
//	TooltipLineID.Vanity => Lang.tip[24].Value,
//	TooltipLineID.VanityLegal => Language.GetTextValue("Misc.CanBePlacedInVanity"),
//	TooltipLineID.Defense => "{0}" + Lang.tip[25].Value,
//	TooltipLineID.PickPower => "{0}" + Lang.tip[26].Value,
//	TooltipLineID.AxePower => "{0}" + Lang.tip[27].Value,
//	TooltipLineID.HammerPower => "{0}" + Lang.tip[28].Value,
//	TooltipLineID.TileBoost => "{0}" + Lang.tip[54].Value,
//	TooltipLineID.HealLife => Language.GetTextValue("CommonItemTooltip.RestoresLife"),
//	TooltipLineID.HealMana => Language.GetTextValue("CommonItemTooltip.RestoresMana"),
//	TooltipLineID.UseMana => Language.GetTextValue("CommonItemTooltip.UsesMana"),
//	TooltipLineID.Placeable => Lang.tip[33].Value,
//	TooltipLineID.Ammo => Lang.tip[34].Value,
//	TooltipLineID.Consumable => Lang.tip[35].Value,
//	TooltipLineID.Material => Lang.tip[36].Value,
//	TooltipLineID.Tooltip => "{0}",
//	TooltipLineID.EtherianManaWarning => Lang.misc[104].Value,
//	TooltipLineID.WellFedExpert => Lang.misc[40].Value,
//	TooltipLineID.BuffTime => Language.GetTextValue("CommonItemTooltip.SecondDuration"),
//	TooltipLineID.OneDropLogo => " ",
//	TooltipLineID.PrefixDamage => "{0}" + Lang.tip[39].Value,
//	TooltipLineID.PrefixSpeed => "{0}" + Lang.tip[40].Value,
//	TooltipLineID.PrefixCritChance => "{0}" + Lang.tip[41].Value,
//	TooltipLineID.PrefixUseMana => "{0}" + Lang.tip[42].Value,
//	TooltipLineID.PrefixSize => "{0}" + Lang.tip[43].Value,
//	TooltipLineID.PrefixShootSpeed => "{0}" + Lang.tip[44].Value,
//	TooltipLineID.PrefixKnockback => "{0}" + Lang.tip[45].Value,
//	TooltipLineID.PrefixAccDefense => "{0}" + Lang.tip[25].Value,
//	TooltipLineID.PrefixAccMaxMana => "{0}" + Lang.tip[31].Value,
//	TooltipLineID.PrefixAccCritChance => "{0}" + Lang.tip[5].Value,
//	TooltipLineID.PrefixAccDamage => "{0}" + Lang.tip[39].Value,
//	TooltipLineID.PrefixAccMoveSpeed => "{0}" + Lang.tip[46].Value,
//	TooltipLineID.PrefixAccMeleeSpeed => "{0}" + Lang.tip[47].Value,
//	TooltipLineID.SetBonus => Lang.tip[48].Value + " {0}",
//	TooltipLineID.Expert => Language.GetText("GameUI.Expert").Value,
//	TooltipLineID.Master => Language.GetTextValue("GameUI.Master"),
//	TooltipLineID.JourneyResearch => Language.GetTextValue("CommonItemTooltip.CreativeSacrificeNeeded"),
//	TooltipLineID.BestiaryNotes => "{0}",
//	TooltipLineID.SpecialPrice => "{0}",
//	TooltipLineID.Price => " {0} " + Lang.inter[15].Value + " {1} " + Lang.inter[16].Value + " {2} " + Lang.inter[17].Value + " {3} " + Lang.inter[18].Value + " ",
//	_ => throw new UsageException("Use TooltipLineData(Mod, string, string) for modded tooltips"),
//};