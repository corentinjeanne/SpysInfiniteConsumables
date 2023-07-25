using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class Count : MultyChoice<int> {

    public Count() { }

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

    public static implicit operator Count(int count) => new() { Value = count };
}