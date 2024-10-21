using System.ComponentModel;
using Terraria.ModLoader;
using SpikysLib;
using Terraria;
using Terraria.Localization;
using SPIC.Configs;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Newtonsoft.Json;
using SpikysLib.Configs;

namespace SPIC.Default.Displays;

public interface ITooltipLineDisplay {
    (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int consumable);
}
public interface ICountToString {
    string CountToString(int consumable, long value, long outOf);
}

public sealed class TooltipConfig {
    [DefaultValue(true)] public bool displayRequirement = true;
    [DefaultValue(true)] public bool coloredLines = true;
    [DefaultValue(0.75f)] public float missingLinesOpacity = 0.75f;
    [DefaultValue(false)] public bool displayDebug = false;

    // Compatibility version < v4.0
    [JsonProperty, DefaultValue(true)] private bool AddMissingLines { set => ConfigHelper.MoveMember(value != true, _ => missingLinesOpacity = 0f); }
}

public sealed class Tooltip : Display, IConfigProvider<TooltipConfig> {
    public static Tooltip Instance = null!;

    public TooltipConfig Config { get; set; } = null!;

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int consumable, IInfinity infinity) => infinity is ITooltipLineDisplay ttl ? ttl.GetTooltipLine(item, consumable) : DefaultTooltipLine(infinity);
    public static string CountToString(int consumable, long value, long outOf, IConsumableInfinity infinity) => infinity is ICountToString cts ? cts.CountToString(consumable, value, outOf) : DefaultCount(value, outOf);
    public static string CountToString(int consumable, long value, IConsumableInfinity infinity) => CountToString(consumable, value, 0, infinity);

    public static (TooltipLine, TooltipLineID?) DefaultTooltipLine(IInfinity infinity) => (new(infinity.Mod, infinity.Name, infinity.DisplayName.Value), null);
    public static string DefaultCount(long owned, long value) => owned == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value) : $"{owned}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value)}";

    public static void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        foreach (var display in InfinityManager.GetDisplayedInfinities(item).SelectMany(displays => displays)) ModifyTooltips(item, tooltips, display.Value, display.Infinity);
    }

    public static void ModifyTooltips(Item item, List<TooltipLine> tooltips, InfinityValue value, IInfinity infinity) {
        (TooltipLine lineToFind, TooltipLineID? position) = GetTooltipLine(item, value.Consumable, infinity);

        TooltipLine? line = tooltips.FindLine(lineToFind.Name);
        if (line is null && Instance.Config.missingLinesOpacity <= 0f) return;
        bool added = false;
        TooltipLine GetLine() {
            if (line is not null) return line;
            tooltips.AddLine(lineToFind, position);
            added = true;
            return line = lineToFind;
        }
        if (value.Infinity > 0) {
            GetLine();
            if (Instance.Config.coloredLines) line!.OverrideColor = infinity.Color;
            line!.Text = value.Infinity == value.Count ?
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Infinite", line.Text) :
                Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.PartiallyInfinite", line.Text, CountToString(value.Consumable, value.Infinity, infinity.Consumable));
        } else if (Instance.Config.displayRequirement) {
            GetLine().Text += value.Count == 0 ?
                $" ({CountToString(value.Consumable, value.Requirement, infinity.Consumable)})" :
                $" ({CountToString(value.Consumable, value.Count, value.Requirement, infinity.Consumable)})";
        }
        if (Instance.Config.displayDebug) {
            var values = InfinityManager.GetDebugInfo(value.Consumable, infinity);
            if (values.Count > 0) GetLine().Text += $" ({string.Join(", ", values)})";
        }
        if (added) line!.OverrideColor = (line.OverrideColor ?? Color.White) * Instance.Config.missingLinesOpacity;
    }
}
