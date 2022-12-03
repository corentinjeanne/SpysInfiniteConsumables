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
/// DO NOT use for config, use <see cref="ItemCountWrapper"/> instead
/// </summary>
public struct ItemCount : ICount<ItemCount> {

    public ItemCount() {
        Type = 0;
        Value = 0L;
        MaxStack = 0;
    }
    public ItemCount(ItemCount other) {
        Type = other.Type;
        Value = other.Value;
        MaxStack = other.MaxStack;
    }
    public ItemCount(Item item, long items) {
        Type = item.type;
        Value = items;
        MaxStack = item.maxStack;
    }
    public ItemCount(Item item, float stacks) {
        Type = item.type;
        Value = stacks;
        MaxStack = item.maxStack;
    }
    internal ItemCount(int type, object value, int maxStack) {
        if (value is not (long or float)) throw new System.ArgumentException("The type of value must be string or long");
        Type = type;
        Value = value;
        MaxStack = maxStack;
    }

    public int Type { get; init; }
    public object Value { get; init; }
    public int MaxStack { get; set; }

    public bool IsNone => Items == 0;
    public bool UseItems => Value is not float;
    public long Items => UseItems ? (long)Value : (long)(Stacks * MaxStack);
    public float Stacks => !UseItems ? (float)Value : (float)Items / MaxStack;

    public ItemCount None => new (this) { Value = 0L };

    public ItemCount Multiply(float value) => UseItems ? new ItemCount(this) { Value = (long)(Items * value) } : new ItemCount(this) { Value = Stacks * value };
    public ItemCount Add(ItemCount count) => UseItems ? new ItemCount(this) { Value = Items + count.Items } : new ItemCount(this) { Value = Stacks + count.Stacks };

    public ItemCount AdaptTo(ItemCount reference) {
        ItemCount other = reference;
        object value = UseItems ? (object)System.Math.Min(Items, other.MaxStack) : (Stacks * System.MathF.Min(1.0f, (float)MaxStack / other.MaxStack)); // TODO reduce boxing
        return new ItemCount(other) { Value = value };
    }
    public int CompareTo(ItemCount other) {
        return UseItems ? Items.CompareTo(other.Items) : AdaptTo(other).Stacks.CompareTo(other.Stacks);
    }

    public string Display(Config.InfinityDisplay.CountStyle style) => style switch {
        Config.InfinityDisplay.CountStyle.Sprite => $"{Items}[i:{Type}]",
        _ or Config.InfinityDisplay.CountStyle.Name => $"{Items} items",
    };
    public string DisplayRawValue(Config.InfinityDisplay.CountStyle style) => Items.ToString();
    public override string ToString() => $"{(UseItems? $"Items={Items}" : $"Stacks={Stacks}")}, MaxStack={MaxStack}";

    public float Ratio(ItemCount other) => UseItems ? (float)Items / other.Items : Stacks / other.Stacks;
}