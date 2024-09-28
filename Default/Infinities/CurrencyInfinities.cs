using System.ComponentModel;
using SPIC.Configs;
using SpikysLib;
using SpikysLib.Configs.UI;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace SPIC.Default.Infinities;
public enum CurrencyCategory {
    None,
    Coins,
    SingleCoin,
}

[CustomModConfigItem(typeof(ObjectMembersElement))]
public sealed class CurrencyRequirements {
    [DefaultValue(CoinsMultiplier)] public float coinsMultiplier = CoinsMultiplier;
    [DefaultValue(CoinsRequirement)] public int coinsRequirement = CoinsRequirement;
    [DefaultValue(SingleMultiplier)] public float singleMultiplier = SingleMultiplier;
    [DefaultValue(SingleRequirement)] public int singleRequirement = SingleRequirement;

    public const int CoinsRequirement = 1_00_00;
    public const float CoinsMultiplier = 1f / 20;
    public const int SingleRequirement = 20;
    public const float SingleMultiplier = 1f/5;
}

public abstract class CurrencyInfinity: Infinity<int, CurrencyCategory>, IConfigProvider<CurrencyRequirements>{
    public sealed override ConsumableInfinity<int> Consumable => Currency.Instance;

    public CurrencyRequirements Config { get; set; } = null!;

    public sealed override long GetRequirement(CurrencyCategory category) => category switch {
        CurrencyCategory.Coins => Config.coinsRequirement,
        CurrencyCategory.SingleCoin => Config.singleRequirement,
        _ => 0,
    };
    public float GetMultiplier(CurrencyCategory category) => category switch {
        CurrencyCategory.Coins => Config.coinsMultiplier,
        CurrencyCategory.SingleCoin => Config.singleMultiplier,
        _ => 1,
    };
    
    protected sealed override CurrencyCategory GetCategoryInner(int consumable) {
        if (consumable == CurrencyHelper.Coins) return CurrencyCategory.Coins;
        if (!CustomCurrencyManager.TryGetCurrencySystem(consumable, out var system)) return CurrencyCategory.None;
        return system.ValuePerUnit().Count == 1 ? CurrencyCategory.SingleCoin : CurrencyCategory.Coins;
    }
    protected sealed override void ModifyInfinity(int consumable, ref long infinity) => infinity = (long)(infinity * GetMultiplier(InfinityManager.GetCategory(consumable, this)));

    protected override void ModifyDisplayedInfinity(Item item, int consumable, ref InfinityVisibility visibility, ref InfinityValue value) {
        int index = System.Array.FindIndex(Main.LocalPlayer.inventory, 0, i => i.IsSimilar(item));
        if (50 <= index && index < 54) visibility = InfinityVisibility.Exclusive;
    }
}

// TODO PreventItemDuplication
public sealed class Shop : CurrencyInfinity {
    public static Shop Instance = null!;
    public sealed override InfinityDefaults Defaults => new() { Color = Colors.CoinGold };
}

public sealed class Purchase : CurrencyInfinity {
    public static Purchase Instance = null!;
    public sealed override InfinityDefaults Defaults => new() { Color = new(78, 78, 228) };
}

