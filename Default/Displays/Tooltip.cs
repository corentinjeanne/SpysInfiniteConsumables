using System.ComponentModel;
using Terraria.ModLoader;
using SpikysLib;
using Terraria;
using Terraria.Localization;
using System.Collections.Generic;
using Terraria.ModLoader.Core;

namespace SPIC.Default.Displays;

public class TooltipDisplay : Component<IInfinity> {
    public TooltipDisplay(TooltipProviderFn? tooltipProvider = null, CountToStringFn? countToString = null) {
        _tooltip = tooltipProvider;
        _toString = countToString;
    }

    public override void Bind() {
        if (LoaderUtils.HasOverride(this, i => i.GetTooltipLine) || _tooltip is not null) Tooltip.SetTooltipLineProvider(Infinity, GetTooltipLine);
        if (LoaderUtils.HasOverride(this, i => i.CountToString) || _toString is not null) Tooltip.SetCountToString(Infinity, CountToString);
    }

    public virtual (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int consumable) => _tooltip!(item, consumable);
    public virtual string CountToString(int consumable, long value, long outOf) => _toString!(consumable, value, outOf);

    private TooltipProviderFn? _tooltip;
    private CountToStringFn? _toString;

    public delegate (TooltipLine, TooltipLineID?) TooltipProviderFn(Item item, int consumable);
    public delegate string CountToStringFn(int consumable, long value, long outOf);
}

public sealed class TooltipConfig {
    [DefaultValue(true)] public bool AddMissingLines = true;
}

public sealed class Tooltip : Display, IConfigurableDisplay<TooltipConfig> {
    public static Tooltip Instance = null!;

    public static (TooltipLine, TooltipLineID?) GetTooltipLine(Item item, int consumable, IInfinity infinity) => s_tooltipProviders.TryGetValue(infinity, out var getter) ? getter(item, consumable) : DefaultTooltipLine(infinity);
    public static string CountToString(int consumable, long value, long outOf, IInfinity infinity) => s_countToStrings.TryGetValue(InfinityManager.IdInfinity(infinity), out var getter) ? getter(consumable, value, outOf) : DefaultCount(value, outOf);
    public static string CountToString(int consumable, long value, IInfinity infinity) => CountToString(consumable, value, 0, infinity);

    public static (TooltipLine, TooltipLineID?) DefaultTooltipLine(IInfinity infinity) => (new(infinity.Mod, infinity.Name, infinity.DisplayName.Value), null);
    public static string DefaultCount(long owned, long value) => owned == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value) : $"{owned}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Count", value)}";

    public static void SetTooltipLineProvider(IInfinity infinity, TooltipDisplay.TooltipProviderFn provider) => s_tooltipProviders[infinity] = provider;
    public static void SetCountToString(IInfinity infinity, TooltipDisplay.CountToStringFn provider) => s_countToStrings[infinity] = provider;

    public override void Unload() {
        s_tooltipProviders.Clear();
        s_countToStrings.Clear();
    }

    private static readonly Dictionary<IInfinity, TooltipDisplay.TooltipProviderFn> s_tooltipProviders = [];
    private static readonly Dictionary<IInfinity, TooltipDisplay.CountToStringFn> s_countToStrings = [];
}
