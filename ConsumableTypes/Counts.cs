using System.Collections.Generic;
using SPIC.Configs;
using SPIC.VanillaGroups;
using Terraria;

namespace SPIC.ConsumableGroup;

public readonly struct CurrencyCount : ICount<CurrencyCount> {

    public CurrencyCount() {
        Currency = CurrencyHelper.None;
        Value = 0;
    }
    public CurrencyCount(int currency, long value = 0) {
        Currency = currency;
        Value = value;
    }
    public CurrencyCount(CurrencyCount other) :this(other.Currency, other.Value) {}

    public int Currency { get; }
    public long Value { get; init; }
    
    public CurrencyCount None => new(Currency);
    public bool IsNone => Value == 0;

    public CurrencyCount Add(CurrencyCount count) => new (this) { Value = Value + count.Value };
    public CurrencyCount Multiply(float value) => new (this) { Value = (long)(Value * value) };
    public float Ratio(CurrencyCount other) => (float)Value / other.Value;

    public CurrencyCount AdaptTo(CurrencyCount reference) => new (reference) { Value = Value };


    public int CompareTo(CurrencyCount other) => Value.CompareTo(other.Value);

    public string Display(InfinityDisplay.CountStyle style) {
        switch (style){
        case InfinityDisplay.CountStyle.Sprite:
            List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(Currency, Value);
            List<string> parts = new();
            foreach ((int type, long count) in items) parts.Add($"{count}[i:{type}]");
            return string.Join(" ", parts);
        case InfinityDisplay.CountStyle.Name or _:
        default:
            return CurrencyHelper.PriceText(Currency, Value);
        }
    }
    public string DisplayRawValue(InfinityDisplay.CountStyle style)
        => InfinityManager.GetCategory(Currency, VanillaGroups.Currency.Instance) == CurrencyCategory.SingleCoin ? $"{Value}" : Display(style);
}

/// <summary>
/// DO NOT use for config, use <see cref="ItemCountWrapper"/> instead
/// </summary>

[CustomWrapper(typeof(ItemCountWrapper))]
public readonly struct ItemCount : ICount<ItemCount> {

    public ItemCount(){
        Type = 0;
        _maxStack = 999;
        _items = 0;
        UseStacks = false;
    }
    public ItemCount(int type, int maxStack = 999) {
        Type = type;
        _maxStack = maxStack;
        _items = 0;
        UseStacks = false;
    }
    public ItemCount(Item item) : this(item.type, item.maxStack) {
        Items = item.stack;
    }
    public ItemCount(ItemCount other) : this(other.Type, other.MaxStack) {
        _items = other._items;
        UseStacks = other.UseStacks;
    }
    public int Type { get; }
    public int MaxStack {
        get => _maxStack;
        init {
            if (UseStacks) _items = (long)(Stacks * value);
            _maxStack = value;
        }
    }

    public bool UseStacks { get; private init; }
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

    private readonly int _maxStack;
    private readonly long _items;

    public bool IsNone => Items == 0;
    public ItemCount None => new(Type, MaxStack);


    long ICount<ItemCount>.Value {
        init {
            if(value < 0) Stacks = -value;
            else Items = value;
        }
    }

    public ItemCount Multiply(float value) => UseStacks ? new ItemCount(this) { Stacks = Stacks * value } : new ItemCount(this) { Items = (long)(Items * value) };
    public ItemCount Add(ItemCount count) => UseStacks ? new ItemCount(this) { Stacks = Stacks + count.AdaptTo(this).Stacks } : new ItemCount(this) { Items = Items + count.Items };

    public ItemCount AdaptTo(ItemCount reference) {
        if(UseStacks){
            float stacks = Stacks * System.MathF.Min(1.0f, (float)MaxStack / reference.MaxStack);
            return new(reference) { Stacks = stacks };
        } else {
            return new ItemCount(reference) { Items = Items };
        }        
    }
    public float Ratio(ItemCount other) => UseStacks ? Stacks / other.Stacks : (float)Items / other.Items;
    public int CompareTo(ItemCount other) => UseStacks ? AdaptTo(other).Stacks.CompareTo(other.Stacks) : Items.CompareTo(other.Items);

    public string Display(InfinityDisplay.CountStyle style) => style switch {
        InfinityDisplay.CountStyle.Sprite => $"{Items}[i:{Type}]",
        _ or InfinityDisplay.CountStyle.Name => $"{Items} items",
    };
    public string DisplayRawValue(InfinityDisplay.CountStyle style) => Items.ToString();
    public override string ToString() => $"{(UseStacks ? $"Stacks={Stacks}" : $"Items={Items}")}, MaxStack={MaxStack}";
}