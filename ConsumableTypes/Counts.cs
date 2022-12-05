using System.Collections.Generic;
using SPIC.VanillaGroups;
using Terraria;

namespace SPIC.ConsumableGroup;

public struct CurrencyCount : ICount<CurrencyCount> {

    public CurrencyCount() {
        Currency = -2;
        Value = 0;
    }
    public CurrencyCount(CurrencyCount other) {
        Currency = other.Currency;
        Value = other.Value;
    }
    public CurrencyCount(int currency, long value) {
        Currency = currency;
        Value = value;
    }

    public int Currency { get; init; }
    public long Value { get; init; }
    public bool IsNone => Value == 0;

    public CurrencyCount Add(CurrencyCount count) => new (this) { Value = Value + (count).Value };
    public CurrencyCount Multiply(float value) => new (this) { Value = (long)(Value * value) };

    public CurrencyCount AdaptTo(CurrencyCount reference) => new (reference) { Value = Value };


    public CurrencyCount None => new(Currency, 0);

    public int CompareTo(CurrencyCount other) => Value.CompareTo(other.Value);

    public string Display(Config.InfinityDisplay.CountStyle style) {
        switch (style){
        case Config.InfinityDisplay.CountStyle.Sprite:
            List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(Currency, Value);
            List<string> parts = new();
            foreach ((int type, long count) in items) parts.Add($"{count}[i:{type}]");
            return string.Join(" ", parts);
        case Config.InfinityDisplay.CountStyle.Name or _:
        default:
            return CurrencyHelper.PriceText(Currency, Value);
        }
    }
    public string DisplayRawValue(Config.InfinityDisplay.CountStyle style)
        => InfinityManager.GetCategory(Currency, VanillaGroups.Currency.Instance) == CurrencyCategory.SingleCoin ? $"{Value}" : Display(style);

    public float Ratio(CurrencyCount other) => (float)Value / other.Value;
}

/// <summary>
/// DO NOT use for config, use <see cref="Config.ItemCountWrapper"/> instead
/// </summary>
public struct ItemCount : ICount<ItemCount> {

    public ItemCount(ItemCount other) {
        Type = other.Type;
        _maxStack = other.MaxStack;
        _items = other.Items;
        UseStacks = other.UseStacks;
    }
    public ItemCount(int type, int maxStack) {
        Type = type;
        _maxStack = maxStack;
        _items = 0;
        UseStacks = false;
    }
    public ItemCount(Item item) {
        Type = item.type;
        _maxStack = item.maxStack;
        _items = 0;
        UseStacks = false;
    }

    public int Type { get; private set; }
    public int MaxStack {
        get => _maxStack;
        init {
            if (UseStacks) _items = (long)(Stacks * value);
            _maxStack = value;
        }
    }

    public long Items {
        get => _items;
        init {
            _items = value;
            UseStacks = false;
        }
    }
    public float Stacks {
        get => (float)_items / MaxStack;
        init {
            _items = (long)(MaxStack*value);
            UseStacks = true;
        }
    }
    public bool UseStacks { get; private set; }    

    private long _items;
    private int _maxStack;

    public bool IsNone => Items == 0;
    public ItemCount None => new(Type, MaxStack);

    public ItemCount Multiply(float value) => UseStacks ? new ItemCount(this) { Stacks = Stacks * value } : new ItemCount(this) { Items = (long)(Items * value) };
    public ItemCount Add(ItemCount count) => UseStacks ? new ItemCount(this) { Stacks = Stacks + count.Stacks } : new ItemCount(this) { Items = Items + count.Items };

    public ItemCount AdaptTo(ItemCount reference) {
        if(UseStacks){
            float stacks = Stacks * System.MathF.Min(1.0f, (float)MaxStack / reference.MaxStack);
            return new(reference) { Stacks = stacks };
        } else {
            long items = System.Math.Min(Items, reference.MaxStack);
            return new ItemCount(reference) { Items = items };
        }        
    }
    public float Ratio(ItemCount other) => UseStacks ? Stacks / other.Stacks : (float)Items / other.Items;
    public int CompareTo(ItemCount other) => UseStacks ? AdaptTo(other).Stacks.CompareTo(other.Stacks) : Items.CompareTo(other.Items);

    public string Display(Config.InfinityDisplay.CountStyle style) => style switch {
        Config.InfinityDisplay.CountStyle.Sprite => $"{Items}[i:{Type}]",
        _ or Config.InfinityDisplay.CountStyle.Name => $"{Items} items",
    };
    public string DisplayRawValue(Config.InfinityDisplay.CountStyle style) => Items.ToString();
    public override string ToString() => $"{(UseStacks ? $"Stacks={Stacks}" : $"Items={Items}")}, MaxStack={MaxStack}";

}