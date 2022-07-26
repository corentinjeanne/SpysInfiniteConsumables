using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace SPIC.Globals;

public class InfinityPlayer : ModPlayer {

    private readonly Dictionary<int, Categories.TypeInfinities> _typeInfinities = new();
    private readonly Dictionary<int, long> _currencyinfinities = new();
    public void ClearInfinities() {
        _typeInfinities.Clear();
        _currencyinfinities.Clear();
    }
    public void UpdateTypeInfinities(Item item) => _typeInfinities[item.type] = GetTypeInfinities(item);

    public Categories.TypeInfinities GetTypeInfinities(int type) => _typeInfinities.ContainsKey(type) ? _typeInfinities[type] : GetTypeInfinities(new Item(type));
    public Categories.TypeInfinities GetTypeInfinities(Item item) {
        if (!_typeInfinities.ContainsKey(item.type)) _typeInfinities[item.type] = new(Player, item);
        return _typeInfinities[item.type];
    }

    public long GetCurrencyInfinity(int currency) {
        if (!_currencyinfinities.ContainsKey(currency)) _currencyinfinities[currency] = CurrencyExtension.GetCurrencyInfinity(Player, currency);
        return _currencyinfinities[currency];
    }

    public override void Load() {
        ClearInfinities();
    }

    public override void Unload() {
        ClearInfinities();
    }
    public override void PreUpdate() {
        InfinityDisplayItem.IncrementCounters();
    }

    public bool HasFullyInfiniteMaterial(Item item) => GetTypeInfinities(item).Material == -2 || GetTypeInfinities(item).Material >= Systems.InfiniteRecipe.HighestCost(item.type);
    public bool HasFullyInfiniteCurrency(int currency) => GetCurrencyInfinity(currency) == -2 || GetCurrencyInfinity(currency) >= (Main.npcShop == 0 ? ConsumptionItem.HighestItemValue : SpicNPC.HighestPrice(currency));

    public bool HasFullyInfinite(Item item)
        => GetTypeInfinities(item).AllInfinite && HasFullyInfiniteMaterial(item) && HasFullyInfiniteCurrency(item.CurrencyType());
}
