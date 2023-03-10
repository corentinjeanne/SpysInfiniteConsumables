using SPIC.ConsumableGroup;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class ItemCountWrapper : MultyChoice<int>{

    public ItemCountWrapper(int maxStack = 999) {
        this.maxStack = maxStack;
    }

    [Choice, Label("$Mods.SPIC.Configs.UI.Disabled.Name")]
    public object? Disabled => null;

    [Choice, Range(1, 9999), Label("$Mods.SPIC.Configs.UI.Items.Name")]
    public int Items {
        get => Value;
        set {
            defaultValue ??= value;
            Value = value;
        }
    }
    [Choice, Range(1, 50), Label("$Mods.SPIC.Configs.UI.Stacks.Name")]
    public int Stacks {
        get => -Value;
        set {
            defaultValue ??= -value;
            Value = -value;
        }
    }

    private int? defaultValue;

    public int maxStack;

    public static implicit operator ItemCount(ItemCountWrapper count) => count.Value >= 0 ? (new(0, count.maxStack) { Items = count.Value }) : (new(0, count.maxStack) { Stacks = -count.Value });

    public override string ChooseProperty() => Value switch {
        0 => nameof(Disabled),
        < 0 => nameof(Stacks),
        _ => nameof(Items)
    };

    public override void ChoiceChange(string from, string to) {
        if(to == nameof(Disabled)){
            Value = 0;
            return;
        }

        if(from == nameof(Disabled)){
            Value = (defaultValue ??= 0);
        }

        if(to == nameof(Items) && Value < 0){
            Value = Stacks * maxStack;
        } else if(to == nameof(Stacks) && Value > 0){
            Value = -(Items+maxStack-1) / maxStack;
        }
    }
}

public class ItemWrapper : MultyChoice<int> {

    public ItemWrapper() {}

    [Choice, Label("$Mods.SPIC.Configs.UI.Disabled.Name")]
    public object? Disabled => null;

    [Choice, Range(1, 9999), Label("$Mods.SPIC.Configs.UI.Items.Name")]
    public int Items {
        get => Value;
        set {
            defaultValue ??= value;
            Value = value;
        }
    }
    private int? defaultValue;

    public int maxStack;

    public static implicit operator ItemCount(ItemWrapper count) => new(0, count.maxStack) { Items = count.Value };

    public override string ChooseProperty() => Value switch {
        0 => nameof(Disabled),
        _ => nameof(Items)
    };

    public override void ChoiceChange(string from, string to) {
        if (to == nameof(Disabled)) {
            Value = 0;
            return;
        }

        if (from == nameof(Disabled)) {
            Value = (defaultValue ??= 0);
        }
    }
}