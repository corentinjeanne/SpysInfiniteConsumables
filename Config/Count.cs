using Terraria.ModLoader.Config;

namespace SPIC.Configs;

public class Count : MultyChoice<int> {

    [Choice]
    public UI.Text Disabled => new();

    [Choice, Range(1, 9999)]
    public int Amount { get; set; }

    public override int Value {
        get => Choice == nameof(Amount) ? Amount : 0;
        set {
            if (value > 0) {
                Amount = value;
                Choice = nameof(Amount);
            }
            else Choice = nameof(Disabled);
        }
    }

    public static implicit operator Count(int count) => new() { Value = count };
}