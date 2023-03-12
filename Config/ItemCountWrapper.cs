using SPIC.ConsumableGroup;
using Terraria.ModLoader.Config;

namespace SPIC.Configs;


public class ItemCountWrapper : MultyChoice<int?> {

    public ItemCountWrapper(int maxStack = 999, bool swapping = true) {
        _removed = swapping;
        MaxStack = maxStack;
    }

    [Choice, Label("$Mods.SPIC.Configs.UI.Default.Name")]
    public string? Default => "Def";
    
    [Choice, Label("$Mods.SPIC.Configs.UI.Disabled.Name")]
    public object? Disabled => null;

    [Choice, Range(1, 9999), Label("$Mods.SPIC.Configs.UI.Items.Name")]
    public int Items {
        get => _items;
        set {
            if (!_removed) {
                _choices.RemoveAt(_choices.FindIndex(p => p.Name == nameof(Stacks)));
                _removed = true;
            }
            Select(nameof(Items));
           _items = value;
        }
    }
    [Choice, Range(1, 50), Label("$Mods.SPIC.Configs.UI.Stacks.Name")]
    public int Stacks {
        get => (_items+MaxStack-1) / MaxStack;
        set {
            if (!_removed) {
                _choices.RemoveAt(_choices.FindIndex(p => p.Name == nameof(Items)));
                _removed = true;
            }
            Select(nameof(Stacks));
            _items = value * MaxStack;
        }
    }

    public int MaxStack { get; init; }

    public override int? Value {
        get => Choices[ChoiceIndex].Name switch {
            nameof(Disabled) => 0,
            nameof(Items) => Items,
            nameof(Stacks) => -Stacks,
            nameof(Default) or _ => null

        };
        set {
            switch (value) {
            case 0:
                Select(nameof(Disabled));
                break;
            case > 0:
                Items = value.Value;
                break;
            case < 0:
                Stacks = -value.Value;
                break;
            case null or _:
                Select(nameof(Default));
                break;
            }
        }
    }

    private int _items;
    private bool _removed;

    public static implicit operator ItemCount(ItemCountWrapper count) => count.Choices[count.ChoiceIndex].Name switch {
        nameof(Disabled) => new(Terraria.ID.ItemID.None, count.MaxStack),
        nameof(Stacks) => new(Terraria.ID.ItemID.None, count.MaxStack) { Stacks = count.Stacks },
        nameof(Items) or _ => new(Terraria.ID.ItemID.None, count.MaxStack) { Items = count.Items }
    };
}