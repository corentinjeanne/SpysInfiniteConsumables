using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPIC.ConsumableGroup;

namespace SPIC.Configs;

public class ItemCountConverter : JsonConverter<ItemCountWrapper> {

    public override ItemCountWrapper ReadJson(JsonReader reader, System.Type objectType, ItemCountWrapper? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JValue count = (JValue)JToken.Load(reader);

        existingValue ??= new(999);
        int value = (int)(long)count.Value!;
        existingValue.useStacks = value < 0;
        existingValue.value = System.Math.Abs(value);
        return existingValue;
    }

    public override void WriteJson(JsonWriter writer, ItemCountWrapper? value, JsonSerializer serializer) {
        if (value is null) return;
        if (value.useStacks) writer.WriteValue(-value.value);
        else writer.WriteValue(value.value);
    }
}

[Terraria.ModLoader.Config.CustomModConfigItem(typeof(UI.ItemCountElement))]
[JsonConverter(typeof(ItemCountConverter))]
public sealed class ItemCountWrapper {

    public ItemCountWrapper(int maxStack = 999) {
        this.maxStack = maxStack;
    }

    public int value;
    public bool useStacks;
    public int maxStack;

    public int Items {
        set {
            this.value = value;
            useStacks = false;
        }
    }
    public int Stacks {
        set {
            this.value = value;
            useStacks = true;
        }
    }

    public void SwapItemsAndStacks() {
        if (useStacks) value *= maxStack;
        else value = (int)System.MathF.Ceiling((float)value/maxStack);
        useStacks = !useStacks;
    }

    public static implicit operator ItemCount(ItemCountWrapper count) => count.useStacks ? (new(0, count.maxStack) { Stacks = count.value }) : (new(0, count.maxStack) { Items = count.value });
}
