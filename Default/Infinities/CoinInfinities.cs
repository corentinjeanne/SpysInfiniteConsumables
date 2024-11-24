using System.ComponentModel;
using SPIC.Configs;
using SpikysLib;
using SpikysLib.Configs.UI;
using Terraria;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace SPIC.Default.Infinities;

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class RequirementRequirements {
    [DefaultValue(CurrencyRequirements.CoinsMultiplier)] public float multiplier = CurrencyRequirements.CoinsMultiplier;
    [DefaultValue(CurrencyRequirements.CoinsRequirement)] public int requirement = CurrencyRequirements.CoinsRequirement;

}

public abstract class CoinInfinity : Infinity<int>, IConfigProvider<RequirementRequirements> {
    public sealed override ConsumableInfinity<int> Consumable => Currency.Instance;

    public RequirementRequirements Config { get; set; } = null!;

    protected sealed override long GetRequirementInner(int consumable) => consumable == CurrencyHelper.Coins ? Config.requirement : 0;
    protected sealed override void ModifyInfinity(int consumable, ref long infinity) => infinity = (long)(infinity * Config.multiplier);

    protected override void ModifyDisplayedInfinity(Item item, int context, int consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (context == ItemSlot.Context.InventoryCoin) visibility = InfinityVisibility.Exclusive;
    }
}

public sealed class Nurse : CoinInfinity {
    public static Nurse Instance = null!;

    public sealed override InfinityDefaults Defaults => new() { Color = new(255, 51, 118) };
}

public sealed class Reforging : CoinInfinity {
    public static Reforging Instance = null!;
    public sealed override InfinityDefaults Defaults => new() { Color = new(90, 154, 165) };
}