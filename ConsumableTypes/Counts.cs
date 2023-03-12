using System.Collections.Generic;
using SPIC.VanillaGroups;
using Terraria;

namespace SPIC.ConsumableGroup;

public readonly struct CurrencyCount : ICount<CurrencyCount> {

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

    public CurrencyCount Add(CurrencyCount count) => new (this) { Value = Value + count.Value };
    public CurrencyCount Multiply(float value) => new (this) { Value = (long)(Value * value) };

    public CurrencyCount AdaptTo(CurrencyCount reference) => new (reference) { Value = Value };


    public CurrencyCount None => new(Currency, 0);

    public int CompareTo(CurrencyCount other) => Value.CompareTo(other.Value);

    public string Display(Configs.InfinityDisplay.CountStyle style) {
        switch (style){
        case Configs.InfinityDisplay.CountStyle.Sprite:
            List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(Currency, Value);
            List<string> parts = new();
            foreach ((int type, long count) in items) parts.Add($"{count}[i:{type}]");
            return string.Join(" ", parts);
        case Configs.InfinityDisplay.CountStyle.Name or _:
        default:
            return CurrencyHelper.PriceText(Currency, Value);
        }
    }
    public string DisplayRawValue(Configs.InfinityDisplay.CountStyle style)
        => InfinityManager.GetCategory(Currency, VanillaGroups.Currency.Instance) == CurrencyCategory.SingleCoin ? $"{Value}" : Display(style);

    public float Ratio(CurrencyCount other) => (float)Value / other.Value;
}

/// <summary>
/// DO NOT use for config, use <see cref="Configs.ItemCountWrapper"/> instead
/// </summary>
public readonly struct ItemCount : ICount<ItemCount> {

    public ItemCount(int type, int maxStack) {
        Type = type;
        _maxStack = maxStack;
        _items = 0;
        UseStacks = false;
    }
    public ItemCount(Item item) : this(item.type, item.maxStack) {}
    public ItemCount(ItemCount other) {
        Type = other.Type;
        _maxStack = other.MaxStack;
        _items = other.Items;
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
    public bool UseStacks { get; private init; }

    private readonly long _items;
    private readonly int _maxStack;

    public bool IsNone => Items == 0;
    public ItemCount None => new(Type, MaxStack);

    public ItemCount Multiply(float value) => UseStacks ? new ItemCount(this) { Stacks = Stacks * value } : new ItemCount(this) { Items = (long)(Items * value) };
    public ItemCount Add(ItemCount count) => UseStacks ? new ItemCount(this) { Stacks = Stacks + count.AdaptTo(this).Stacks } : new ItemCount(this) { Items = Items + count.Items };

    public ItemCount AdaptTo(ItemCount reference) {
        if(UseStacks){
            float stacks = Stacks * System.MathF.Min(1.0f, (float)MaxStack / reference.MaxStack);
            return new(reference) { Stacks = stacks };
        } else {
            // long items = System.Math.Min(Items, reference.MaxStack);
            return new ItemCount(reference) { Items = Items };
        }        
    }
    public float Ratio(ItemCount other) => UseStacks ? Stacks / other.Stacks : (float)Items / other.Items;
    public int CompareTo(ItemCount other) => UseStacks ? AdaptTo(other).Stacks.CompareTo(other.Stacks) : Items.CompareTo(other.Items);

    public string Display(Configs.InfinityDisplay.CountStyle style) => style switch {
        Configs.InfinityDisplay.CountStyle.Sprite => $"{Items}[i:{Type}]",
        _ or Configs.InfinityDisplay.CountStyle.Name => $"{Items} items",
    };
    public string DisplayRawValue(Configs.InfinityDisplay.CountStyle style) => Items.ToString();
    public override string ToString() => $"{(UseStacks ? $"Stacks={Stacks}" : $"Items={Items}")}, MaxStack={MaxStack}";

}