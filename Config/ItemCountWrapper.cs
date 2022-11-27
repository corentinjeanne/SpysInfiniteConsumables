using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPIC.ConsumableGroup;

namespace SPIC.Config;

public class ItemCountConverter : JsonConverter<ItemCountWrapper> {

    public override ItemCountWrapper ReadJson(JsonReader reader, Type objectType, ItemCountWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue count = (JValue)JToken.Load(reader);

        existingValue ??= new ItemCountWrapper(count.Value!, 999);
        if (count.Value is long v && v >= 0) existingValue.value = v;
        else existingValue.value = -(float)(double)count.Value!;
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, ItemCountWrapper? value, JsonSerializer serializer) {
        if (value is null) return;
        if (value.UseItems) writer.WriteValue((long)value.value);
        else writer.WriteValue(-(float)value.value);
    }
}

[Terraria.ModLoader.Config.CustomModConfigItem(typeof(Config.UI.ItemCountElement))]
[JsonConverter(typeof(ItemCountConverter))]
public sealed class ItemCountWrapper {

    public object value;
    public int maxStack;
    public bool UseItems => value is long;

    public void SwapItemsAndStacks() {
        if (UseItems) value = ((ItemCount)this).Stacks;
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
