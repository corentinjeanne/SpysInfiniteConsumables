using SPIC.Default.Displays;
using SpikysLib;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace SPIC.Default.Infinities;

public class ConsumableItem : ConsumableInfinity<Item>, ICountToString {
    public static ConsumableItem Instance = null!;

    public sealed override InfinityDefaults Defaults => new() { Color = new(209, 138, 138) };
    public sealed override ConsumableDefaults ConsumableDefaults => new() { Preset = Default.Presets.ConsumableDefaults.Instance };

    public override long CountConsumables(Player player, Item consumable) => player.CountItems(consumable.type, true);
    public override int GetId(Item consumable) => consumable.type;
    public override Item ToConsumable(int id) => new(id);
    public override Item ToConsumable(Item item) => item;
    public override ItemDefinition ToDefinition(Item consumable) => new(consumable.type);

    public string CountToString(int consumable, long value, long outOf)
        => outOf == 0 ? Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", value) : $"{value}/{Language.GetTextValue($"{Localization.Keys.CommonItemTooltips}.Items", outOf)}";
}