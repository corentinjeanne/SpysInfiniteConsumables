using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class Count : MultyChoice<int> {

    public Count() : base() {}
    public Count(int value) : base(value) {}

    [Choice]
    public UI.Text Disabled => new();

    [Choice, Range(1, 9999)]
    public int Amount { get; set; }

    public override int Value {
        get => Choice == nameof(Disabled) ? 0 : Amount;
        set {
            Choice = value == 0 ? nameof(Disabled) : nameof(Amount);
            Amount = value;
        }
    }

    public static implicit operator Count(int count) => new() { Value = count };
}

public class Count<TCategory> : Count where TCategory : struct, System.Enum {
    public Count() : base() { }
    public Count(int value) : base(value) { }
    public Count(TCategory category) : base(-System.Convert.ToInt32(category)) { }

    [Choice]
    public TCategory Category { get; set; } = default!;

    public override int Value {
        get => Choice == nameof(Category) ? -System.Convert.ToInt32(Category) : base.Value;
        set {
            if (value >= 0) {
                base.Value = value;
                return;
            }
            Choice = nameof(Category);
            Category = System.Enum.Parse<TCategory>((-value).ToString());
        }
    }
}