using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPIC.ConsumableGroup;

public class ItemCountConverter : JsonConverter<ItemCountWrapper> {

    public override ItemCountWrapper ReadJson(JsonReader reader, Type objectType, ItemCountWrapper existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue count = (JValue)JToken.Load(reader);

        if (!hasExistingValue) existingValue = new ItemCountWrapper(count.Value, 999);
        if (count.Value is long v && v >= 0) existingValue.value = v;
        else existingValue.value = -(float)(double)count.Value;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, ItemCountWrapper value, JsonSerializer serializer) {
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

    public static implicit operator ItemCount(ItemCountWrapper count) => new(count.value, count.maxStack);
}

/// <summary>
/// DO NOT use for config, use <see cref="ItemCountWrapper"/> instead
/// </summary>
public struct ItemCount {
    
    public ItemCount(ItemCount count) {
        Value = count.Value;
        MaxStack = count.MaxStack;
    }
    public ItemCount(long items, int maxStack = 999) {
        Value = items;
        MaxStack = maxStack;
    }
    public ItemCount(float stacks, int maxStack = 999) {
        Value = stacks;
        MaxStack = maxStack;
    }
    internal ItemCount(object value, int maxStack = 999) {
        if (value is not (long or float)) throw new ArgumentException("The type of value must be string or long");
        Value = value;
        MaxStack = maxStack;
    }

    public object Value { get; init; }
    public int MaxStack { get; set; }

    public bool IsNone => Items == 0;
    public bool UseItems => Value is long;
    public long Items => Value is long items ? items : (long)(Stacks * MaxStack);
    public float Stacks => Value is float stacks ? stacks : (float)Items / MaxStack;

    public static ItemCount None => new(0);

    public ItemCount CapMaxStack(int maxStack) {
        if (maxStack >= MaxStack) return new(this);
        if (UseItems) return new(this) { MaxStack = Math.Min(MaxStack, maxStack) };
        long items = Items;
        return new((float)items / maxStack, maxStack);
    }

    public override int GetHashCode() => HashCode.Combine(Value, MaxStack);
    public override bool Equals(object obj) => obj is ItemCount b && this == b;

    public static bool operator ==(ItemCount a, ItemCount b) => b.UseItems ? a.Items == b.Items : a.CapMaxStack(b.MaxStack).Stacks == b.Stacks;
    public static bool operator !=(ItemCount a, ItemCount b) => !(a == b);
    public static bool operator >(ItemCount a, ItemCount b) => b.UseItems ? a.Items > b.Items : a.CapMaxStack(b.MaxStack).Stacks > b.Stacks;
    public static bool operator <(ItemCount a, ItemCount b) => !(a >= b);
    public static bool operator <=(ItemCount a, ItemCount b) => !(a > b);
    public static bool operator >=(ItemCount a, ItemCount b) => a > b || a == b;

    public static ItemCount operator *(ItemCount a, float b) => a.UseItems ? new(a) { Value = (long)(a.Items * b) } : new(a) { Value = a.Stacks * b };
    public static ItemCount operator /(ItemCount a, float b) => a * (1 / b);

    public static ItemCount operator +(ItemCount a, ItemCount b) => a.UseItems ? new(a) { Value = a.Items + b.Items } : new(a) { Value = a.Stacks + b.Stacks };
    public static ItemCount operator -(ItemCount a, ItemCount b) => a + b * -1;
}
