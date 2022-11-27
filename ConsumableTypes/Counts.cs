using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria;

namespace SPIC.ConsumableGroup;

public class ItemCountConverter : JsonConverter<ItemCountWrapper> {

    public override ItemCountWrapper ReadJson(JsonReader reader, Type objectType, ItemCountWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue count = (JValue)JToken.Load(reader);

        existingValue ??= new ItemCountWrapper(count.Value!, 999);
        if (count.Value is long v && v >= 0) existingValue.value = v;
        else existingValue.value = -(float)(double)count.Value!;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, ItemCountWrapper? value, JsonSerializer serializer) {
        if(value is null) return;
        if(value.UseItems)writer.WriteValue((long)value.value);
        else writer.WriteValue(-(float)value.value);
    }
}

[Terraria.ModLoader.Config.CustomModConfigItem(typeof(Configs.UI.ItemCountElement))]
[JsonConverter(typeof(ItemCountConverter))]
public sealed class ItemCountWrapper {

    public object value;
    public int maxStack;
    public bool UseItems => value is long;

    public void SwapItemsAndStacks(){
        if(UseItems) value = ((ItemCount)this).Stacks;
        else value = ((ItemCount)this).Items;
    }

    public ItemCountWrapper(long items, int maxStack = 999) {
        value = items;
        this.maxStack = maxStack;
    }
    public ItemCountWrapper(float stacks, int maxStack = 999) {
        value = stacks;
        this.maxStack = maxStack;
    }
    internal ItemCountWrapper(object value, int maxStack = 999) {
        if (value is not (long or float)) throw new ArgumentException("The type of value must be string or long");
        this.value = value;
        this.maxStack = maxStack;
    }

    public static implicit operator ItemCount(ItemCountWrapper count) => new(0, count.value, count.maxStack);
}

public interface ICount : IComparable<ICount> {
    ICount Multiply(float value);
    ICount Add(ICount count);
    ICount AdaptTo(ICount reference);

    bool IsNone { get; }
    ICount None { get; }

    string DisplayRatio(ICount other);
    string Display();
}

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

    public ICount AdaptTo(ICount reference) => new CurrencyCount((CurrencyCount)reference) { Value = Value};


    public ICount None => new CurrencyCount(Currency, 0);

    public int CompareTo(ICount? other) => Value.CompareTo(((CurrencyCount)other).Value);

    public string Display() {
        List<KeyValuePair<int, long>> items = CurrencyHelper.CurrencyCountToItems(Currency, Value);
        List<string> parts = new();
        foreach((int type, long count) in items) parts.Add($"{count}[i:{type}]");        
        return string.Join(" ",parts);
    }
    public string DisplayRatio(ICount other) => $"{Display()}/{other.Display()}";
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
        if (value is not (long or float)) throw new ArgumentException("The type of value must be string or long");
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
        object value = UseItems ? (object)Math.Min(Items, other.MaxStack) : (Stacks * MathF.Min(1.0f, (float)MaxStack / other.MaxStack));
        return new ItemCount(other) { Value = value };
    }
    public int CompareTo(ICount? other) {
        ItemCount value = (ItemCount)other;
        return value.UseItems ? Items.CompareTo(value.Items) : ((ItemCount)AdaptTo(other)).Stacks.CompareTo(value.Stacks);
    }

    public string Display() => $"{Items} items";
    public string DisplayRatio(ICount other) => $"{Items}/{other.Display()}";

    public override string ToString() => string.Join("", "{ Value=", Value, ", MaxStack=", MaxStack, "}");
}