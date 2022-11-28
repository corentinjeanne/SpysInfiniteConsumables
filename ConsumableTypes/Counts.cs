using System.Collections.Generic;
using SPIC.VanillaConsumableTypes;
using Terraria;

namespace SPIC.ConsumableGroup;

public struct CurrencyCount : ICount {

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

    public ICount Add(ICount count) => new CurrencyCount(this) { Value = Value + ((CurrencyCount)count).Value };
    public ICount Multiply(float value) => new CurrencyCount(this) { Value = (long)(Value * value) };

    public ICount AdaptTo(ICount reference) => new CurrencyCount((CurrencyCount)reference) { Value = Value };


    public ICount None => new CurrencyCount(Currency, 0);

    public int CompareTo(ICount? other) => Value.CompareTo(((CurrencyCount)other).Value);

    public string Display() {
        List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(Currency, Value);
        List<string> parts = new();
        foreach ((int type, long count) in items) parts.Add($"{count}[i:{type}]");
        return string.Join(" ", parts);
    }
    public string DisplayRatio(ICount other) => InfinityManager.GetCategory<int, CurrencyCategory>(Currency, VanillaConsumableTypes.Currency.ID) == CurrencyCategory.Coin ?
        $"{Display()}/{other.Display()}" : $"{Value}/{other.Display()}";
}

/// <summary>
/// DO NOT use for config, use <see cref="ItemCountWrapper"/> instead
/// </summary>
public struct ItemCount : ICount {

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

    public ICount None => new ItemCount(this) { Value = 0L };

    public ICount Multiply(float value) => UseItems ? new ItemCount(this) { Value = (long)(Items * value) } : new ItemCount(this) { Value = Stacks * value };
    public ICount Add(ICount count) => UseItems ? new ItemCount(this) { Value = Items + ((ItemCount)count).Items } : new ItemCount(this) { Value = Stacks + ((ItemCount)count).Stacks };

    public ICount AdaptTo(ICount reference) {
        ItemCount other = (ItemCount)reference;
        object value = UseItems ? (object)System.Math.Min(Items, other.MaxStack) : (Stacks * System.MathF.Min(1.0f, (float)MaxStack / other.MaxStack));
        return new ItemCount(other) { Value = value };
    }
    public int CompareTo(ICount? other) {
        ItemCount value = (ItemCount)other;
        return UseItems ? Items.CompareTo(value.Items) : ((ItemCount)AdaptTo(other)).Stacks.CompareTo(value.Stacks);
    }

    public string Display() => $"{Items} items";
    public string DisplayRatio(ICount other) => $"{Items}/{other.Display()}";
    public override string ToString() => $"{(UseItems? $"Items={Items}" : $"Stacks={Stacks}")}, MaxStack={MaxStack}";
}