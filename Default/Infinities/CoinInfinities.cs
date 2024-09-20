using System.ComponentModel;
using SPIC.Configs;
using SpikysLib;
using Terraria;
using Terraria.ID;

namespace SPIC.Default.Infinities;

public sealed class CoinRequirements {
    [DefaultValue(CurrencyRequirements.CoinsMultiplier)] public float multiplier = CurrencyRequirements.CoinsMultiplier;
    [DefaultValue(CurrencyRequirements.CoinsRequirement)] public int requirement = CurrencyRequirements.CoinsRequirement;

}

public abstract class CoinInfinity : Infinity<int>, IConfigProvider<CoinRequirements> {
    public sealed override ConsumableInfinity<int> Consumable => Currency.Instance;

    public CoinRequirements Config { get; set; } = null!;

    protected sealed override long GetRequirementInner(int consumable) => consumable == CurrencyHelper.Coins ? Config.requirement : 0;
    public sealed override long GetInfinity(int consumable, long count) => (long)(base.GetInfinity(consumable, count) * Config.multiplier);
}

public sealed class Nurse : CoinInfinity {
    public static Nurse Instance = null!;

    public sealed override InfinityDefaults Defaults => new() { Color = new(255, 51, 118) };

    protected override void ModifyDisplayedInfinity(Item item, int consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (Main.LocalPlayer.TalkNPC?.type == NPCID.Nurse) visibility = InfinityVisibility.Exclusive;
        // value = value.For(); // TODO nurse price
    }
}

public sealed class Reforging : CoinInfinity {
    public static Reforging Instance = null!;
    public sealed override InfinityDefaults Defaults => new() { Color = new(90, 154, 165) };

    protected override void ModifyDisplayedInfinity(Item item, int consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        if (Main.InReforgeMenu) visibility = InfinityVisibility.Exclusive;
        // value = value.For(); // TODO reforge price
    }
}