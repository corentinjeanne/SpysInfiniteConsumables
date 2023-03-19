using SPIC.ConsumableGroup;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


public class ItemCountWrapper : MultyChoice<int> {

    public ItemCountWrapper() : this (999, true) {}
    public ItemCountWrapper(int maxStack = 999, bool swapping = true) {
        _antiSwap = swapping;
        MaxStack = maxStack;
    }

    [Choice, Label("$Mods.SPIC.Configs.UI.Disabled.Name")]
    public object? Disabled => null;

    [Choice, Range(1, 9999), Label("$Mods.SPIC.Configs.UI.Items.Name")]
    public int Items {
        get => _items;
        set {
            TryRemove(nameof(Stacks));
            Value = value;
        }
    }
    [Choice, Range(1, 50), Label("$Mods.SPIC.Configs.UI.Stacks.Name")]
    public int Stacks {
        get => (_items+MaxStack-1) / MaxStack;
        set {
            TryRemove(nameof(Items));
            Value = -value;
        }
    }

    private void TryRemove(string s){
        if (_antiSwap) return;
        choices.RemoveAt(choices.FindIndex(p => p.Name == s));
        _antiSwap = true;
    }

    public int MaxStack { get; }

    public override int Value {
        get => Choice.Name switch {
            nameof(Items) => Items,
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
            case 0 or _:
                Select(nameof(Disabled));
                break;
            }
        }
    }

    private int _items;
    private bool _antiSwap;

    public static implicit operator ItemCount(ItemCountWrapper count) => count.Choice.Name switch {
        nameof(Items) => new(Terraria.ID.ItemID.None, count.MaxStack) { Items = count.Items },
        nameof(Stacks) => new(Terraria.ID.ItemID.None, count.MaxStack) { Stacks = count.Stacks },
        nameof(Disabled) or _ => new(Terraria.ID.ItemID.None, count.MaxStack)
    };
}