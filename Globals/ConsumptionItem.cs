using System.Reflection;
using MonoMod.Cil;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using SPIC.ConsumableTypes;
using System;

namespace SPIC.Globals;

public class ConsumptionItem : GlobalItem {

    public override void Load() {
        s_itemMaxStack = new int[ItemID.Count];
        IL.Terraria.Item.SetDefaults_int_bool += Hook_ItemSetDefaults;
    }

    public override void Unload() {
        SetDefaultsHook = false;
        s_itemMaxStack = null;
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

    public override void SetDefaults(Item item) {
        if (item.tileWand != -1) Placeable.RegisterWand(item);
    }

    public override void SetStaticDefaults() {
        System.Array.Resize(ref s_itemMaxStack, ItemLoader.ItemCount);
        for (int type = 0; type < ItemLoader.ItemCount; type++) {
            Item item = new(type);
            if (type >= ItemID.Count || !SetDefaultsHook)
                s_itemMaxStack[type] = Math.Clamp(item.maxStack, 1, 999);
        }
    }

    public override bool ConsumeItem(Item item, Player player) {
        Configs.CategoryDetection detection = Configs.CategoryDetection.Instance;

        DetectionPlayer detectionPlayer = player.GetModPlayer<DetectionPlayer>();

        // LeftClick
        if (detectionPlayer.InItemCheck) {
            // Consumed by other item
            if (item != player.HeldItem) {
                if (detection.DetectMissing && (PlaceableCategory)item.GetCategory(Placeable.ID) == PlaceableCategory.None)
                    Configs.CategoryDetection.Instance.SaveDetectedCategory(item, (byte)PlaceableCategory.Block, Placeable.ID);
                    // Placeable.SaveDetectedCategory(item, (byte)PlaceableCategory.Block);

                return !player.HasInfinite(item, 1, Placeable.ID); //- !infinities.IsInfinite(Placeable.ID) !(settings.InfinitePlaceables && infinities.Placeable > 0) ;
            }

            detectionPlayer.TryDetectCategory();
        } else {
            // RightClick
            if (Main.playerInventory && player.itemAnimation == 0 && Main.mouseRight && Main.mouseRightRelease) {

                if ((GrabBagCategory)item.GetCategory(GrabBag.ID) == GrabBagCategory.Unkown) {
                    if ((UsableCategory)item.GetCategory(Usable.ID) == UsableCategory.Tool)
                        return !player.HasInfinite(item, 1, Usable.ID);

                    if (detection.DetectMissing) Configs.CategoryDetection.Instance.SaveDetectedCategory(item, (byte)GrabBagCategory.Crate, GrabBag.ID);
                }
                return !player.HasInfinite(item, 1, GrabBag.ID);

            }

            // Hotkey
            // ? Hotkeys detect buff
        }

        // LeftClick
        if ((UsableCategory)item.GetCategory(Usable.ID) != UsableCategory.None)
            return !player.HasInfinite(item, 1, Usable.ID);
        if (item.Placeable())
            return !player.HasInfinite(item, 1, Placeable.ID);
        return !player.HasInfinite(item, 1, GrabBag.ID);
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
        => !player.HasInfinite(ammo, 1, Ammo.ID);

    public override bool? CanConsumeBait(Player player, Item bait)
        => !player.HasInfinite(bait, 1, Usable.ID) ?
            null : false;

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
        if (!Main.LocalPlayer.HasInfinite(CurrencyHelper.LowestValueType(-1), reforgePrice, Currency.ID)) return false;
        reforgePrice = 0;
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched) {
        int sacrifices = Main.LocalPlayerCreativeTracker.ItemSacrifices.SacrificesCountByItemIdCache[item.type];
        int researchCost = Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type];
        int consumed = Math.Min(Utils.Clamp(researchCost - sacrifices, 0, researchCost), item.stack);
        if (Main.LocalPlayer.HasInfinite(item, consumed, JourneySacrifice.ID))
            item.stack += consumed;
    }
}
