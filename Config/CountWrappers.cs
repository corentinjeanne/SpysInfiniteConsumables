using SPIC.ConsumableGroup;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;

[System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
public sealed class CustomWrapper : System.Attribute {    
    public System.Type WrapperType { get; }
    public CustomWrapper(System.Type wrapperType) {
        WrapperType = wrapperType;
    }
}

public class UniversalCountWrapper : MultyChoice<int> {

    public UniversalCountWrapper() {}

    [Choice]
    public object? Disabled => null;

    [Choice, Range(1, 9999)]
    public override int Value {
        get => Choice.Name switch {
            nameof(Value) => _value,
            nameof(Disabled) or _ => _value < 0 ? _value : 0,
        };
        set {
            switch (value) {
            case > 0:
                _value = value;
                Select(nameof(Value));
                break;
            case < 0:
                _value = value;
                Select(nameof(Disabled));
                break;
            default:
                Select(nameof(Disabled));
                break;
            }
        }
    }

    private int _value = 1;

    public TCount As<TCount>() where TCount: ICount<TCount>, new() => new() { Value = Value };
}

public class ItemCountWrapper : UniversalCountWrapper {

    public ItemCountWrapper() : this (999, true) {}
    public ItemCountWrapper(int maxStack = 999, bool swapping = true) {
        _antiSwap = swapping;
        MaxStack = maxStack;
    }

    [Choice, Range(1, 50), LabelKey($"${Localization.Keys.UI}.Stacks.Name")]
    public int Stacks {
        get => (_items+MaxStack-1) / MaxStack;
        set {
            TryRemove(nameof(Value));
            Value = -value;
        }
    }

    private void TryRemove(string s){
        if (_antiSwap) return;
        choices.RemoveAt(choices.FindIndex(p => p.Name == s));
        _antiSwap = true;
    }

    [Choice, Range(1, 9999), LabelKey($"${Localization.Keys.UI}.Items.Name")]
    public int Items {
        get => _items;
        set {
            TryRemove(nameof(Stacks));
            Value = value;
        }
    }

    public int MaxStack { get; }

    public override int Value {
        get => Choice.Name switch {
            nameof(Items) => _items,
            nameof(Stacks) => -Stacks,
            nameof(Disabled) or _ => 0,
        };
        set {
            switch (value) {
            case > 0:
                Select(nameof(Items));
                _items = value;
                break;
            case < 0:
                Select(nameof(Stacks));
                _items = -value * MaxStack;
                break;
            default:
                Select(nameof(Disabled));
                break;
            }
        }
    }

    private int _items;
    private bool _antiSwap;

    public static implicit operator ItemCount(ItemCountWrapper count) => count.Choice.Name switch {
        nameof(Items) => new(Terraria.ID.ItemID.None, count.MaxStack) { Items = count.Items},
        nameof(Stacks) => new(Terraria.ID.ItemID.None, count.MaxStack) { Stacks = count.Stacks},
        nameof(Disabled) or _ => new(Terraria.ID.ItemID.None, count.MaxStack)
    };
}