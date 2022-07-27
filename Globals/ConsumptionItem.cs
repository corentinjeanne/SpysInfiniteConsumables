using System.Reflection;
using MonoMod.Cil;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    public override void Load() {
        s_itemMaxStack = new int[ItemID.Count];
        IL.Terraria.Item.SetDefaults_int_bool += Hook_ItemSetDefaults;
        CategoryManager.ClearAll();
    }
    public override void Unload() {
        SetDefaultsHook = false;
        s_itemMaxStack = null;
        CategoryManager.ClearAll();
    }

    public static bool SetDefaultsHook { get; private set; }
    private static int[] s_itemMaxStack;
    public static int MaxStack(int type) => s_itemMaxStack[type];

    private void Hook_ItemSetDefaults(ILContext il) {
        System.Type[] args = { typeof(Item), typeof(bool) };
        MethodBase setdefault_item_bool = typeof(ItemLoader).GetMethod(
            nameof(Item.SetDefaults),
            BindingFlags.Static | BindingFlags.NonPublic,
            args
        );

        // IL code editing
        ILCursor c = new(il);

        if (setdefault_item_bool == null || !c.TryGotoNext(i => i.MatchCall(setdefault_item_bool))) {
            Mod.Logger.Error("Unable to apply patch!");
            SetDefaultsHook = false;
            return;
        }

        c.Index -= args.Length;
        c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
        c.EmitDelegate((Item item) => {
            if (item.type < ItemID.Count) s_itemMaxStack[item.type] = item.maxStack;
        });

        SetDefaultsHook = true;
    }

    public static long HighestItemValue { get; private set; }


    public override void SetDefaults(Item item) {
        if (item.tileWand != -1) PlaceableExtension.RegisterWandAmmo(item);
        if (item.FitsAmmoSlot() && item.mech) PlaceableExtension.RegisterWandAmmo(item.type, Categories.Placeable.Wiring);
    }

    public override void SetStaticDefaults() {
        System.Array.Resize(ref s_itemMaxStack, ItemLoader.ItemCount);
        for (int type = 0; type < ItemLoader.ItemCount; type++) {
            Item item = new(type);
            if (type >= ItemID.Count || !SetDefaultsHook)
                s_itemMaxStack[type] = System.Math.Clamp(item.maxStack, 1, 999);
            if (item.value > HighestItemValue)
                HighestItemValue = item.value;
        }
    }

    // TODO Add functions for every category
    public override bool ConsumeItem(Item item, Player player) {
        Configs.Requirements settings = Configs.Requirements.Instance;
        Configs.CategoryDetection detected = Configs.CategoryDetection.Instance;

        InfinityPlayer infinityPlayer = player.GetModPlayer<InfinityPlayer>();
        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        Categories.TypeCategories categories;
        Categories.TypeInfinities infinities;
        // LeftClick
        if (detectionPlayer.InItemCheck) {
            // Consumed by other item
            if (item != player.HeldItem) {
                categories = item.GetTypeCategories();
                infinities = infinityPlayer.GetTypeInfinities(item);
                if (detected.DetectMissing && categories.Placeable == Categories.Placeable.None)
                    Configs.CategoryDetection.Instance.DetectedPlaceable(item, Categories.Placeable.Block);

                return !(settings.InfinitePlaceables && infinities.Placeable > 0);
            }

            detectionPlayer.TryDetectCategory();
        } else {
            // RightClick
            if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {
                categories = item.GetTypeCategories();
                infinities = infinityPlayer.GetTypeInfinities(item);

                if (!categories.GrabBag.HasValue) {
                    if (categories.Consumable == Categories.Consumable.Tool)
                        return !(settings.InfiniteConsumables && 1 <= infinities.Consumable);


                    if (detected.DetectMissing)
                        Configs.CategoryDetection.Instance.DetectedGrabBag(item);
                }
                return !(settings.InfiniteGrabBags && 1 <= infinities.GrabBag);

            }

            // Hotkey
            // ? Hotkeys detect buff
        }

        // LeftClick
        categories = item.GetTypeCategories();
        infinities = infinityPlayer.GetTypeInfinities(item);
        if (categories.Consumable != Categories.Consumable.None)
            return !(settings.InfiniteConsumables && 1 <= infinities.Consumable);
        if (item.Placeable())
            return !(settings.InfinitePlaceables && 1 <= infinities.Placeable);
        return !(settings.InfiniteGrabBags && 1 <= infinities.GrabBag);
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
        => !(Configs.Requirements.Instance.InfiniteConsumables && 1 <= player.GetModPlayer<InfinityPlayer>().GetTypeInfinities(ammo).Ammo);

    public override bool? CanConsumeBait(Player player, Item bait)
        => !(Configs.Requirements.Instance.InfiniteConsumables && 1 <= player.GetModPlayer<InfinityPlayer>().GetTypeInfinities(bait).Consumable) ?
            null : false;

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        InfinityPlayer infinityPlayer = Main.LocalPlayer.GetModPlayer<InfinityPlayer>();
        if (reforgePrice > infinityPlayer.GetCurrencyInfinity(-1)) return false;
        reforgePrice = 0;
        return true;
    }
}
