using System;
using SPIC.Default.Globals;
using SPIC.Default.Infinities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Globals;

public class InfiniteWire : GlobalTile {
    public override void Load() {
        On_WorldGen.PlaceWire += HookPlaceWire1;
        On_WorldGen.PlaceWire2 += HookPlaceWire2;
        On_WorldGen.PlaceWire3 += HookPlaceWire3;
        On_WorldGen.PlaceWire4 += HookPlaceWire4;
        On_WorldGen.PlaceActuator += HookPlaceActuator;
        On_WorldGen.KillWire += HookKillWire1;
        On_WorldGen.KillWire2 += HookKillWire2;
        On_WorldGen.KillWire3 += HookKillWire3;
        On_WorldGen.KillWire4 += HookKillWire4;
        On_WorldGen.KillActuator += HookKillActuator;
    }

    private static void PlaceInfinite(int i, int j, WireType wire) {
        var world = InfiniteWorld.Instance;
        if (world.IsInfinitePlacementContext()) world.SetInfinite(i, j, wire);
    }

    private static bool HookPlaceWire(Func<int, int, bool> orig, int i, int j, WireType wire) {
        if (!orig(i, j)) return false;
        if (Placeable.PreventItemDuplication) PlaceInfinite(i, j, wire);
        return true;
    }
    private static bool HookKillWire(Func<int, int, bool> orig, int i, int j, WireType wire, Func<Tile, bool> hasWire, Action<Tile> unset) {
        var world = InfiniteWorld.Instance;
        var tile = Main.tile[i, j];
        if (!hasWire(tile) || !Placeable.PreventItemDuplication || !world.IsInfinite(i, j, wire)) return orig(i, j);
        SoundEngine.PlaySound(SoundID.Dig, new(i * 16, j * 16));
        unset(tile);
        for (int k = 0; k < 5; k++) Dust.NewDust(new((float)(i * 16), (float)(j * 16)), 16, 16, 50);
        return true;
    }

    private static bool HookPlaceWire1(On_WorldGen.orig_PlaceWire orig, int i, int j) => HookPlaceWire((i, j) => orig(i, j), i, j, WireType.Red);
    private static bool HookPlaceWire2(On_WorldGen.orig_PlaceWire2 orig, int i, int j) => HookPlaceWire((i, j) => orig(i, j), i, j, WireType.Blue);
    private static bool HookPlaceWire3(On_WorldGen.orig_PlaceWire3 orig, int i, int j) => HookPlaceWire((i, j) => orig(i, j), i, j, WireType.Green);
    private static bool HookPlaceWire4(On_WorldGen.orig_PlaceWire4 orig, int i, int j) => HookPlaceWire((i, j) => orig(i, j), i, j, WireType.Yellow);
    private static bool HookPlaceActuator(On_WorldGen.orig_PlaceActuator orig, int i, int j) => HookPlaceWire((i, j) => orig(i, j), i, j, WireType.Actuator);
    private static bool HookKillWire1(On_WorldGen.orig_KillWire orig, int i, int j) => HookKillWire((i, j) => orig(i, j), i, j, WireType.Red, t => t.RedWire, t => t.RedWire = false);
    private static bool HookKillWire2(On_WorldGen.orig_KillWire2 orig, int i, int j) => HookKillWire((i, j) => orig(i, j), i, j, WireType.Blue, t => t.BlueWire, t => t.BlueWire = false);
    private static bool HookKillWire3(On_WorldGen.orig_KillWire3 orig, int i, int j) => HookKillWire((i, j) => orig(i, j), i, j, WireType.Green, t => t.GreenWire, t => t.GreenWire = false);
    private static bool HookKillWire4(On_WorldGen.orig_KillWire4 orig, int i, int j) => HookKillWire((i, j) => orig(i, j), i, j, WireType.Yellow, t => t.YellowWire, t => t.YellowWire = false);
    private static bool HookKillActuator(On_WorldGen.orig_KillActuator orig, int i, int j) => HookKillWire((i, j) => orig(i, j), i, j, WireType.Actuator, t => t.HasActuator, t => t.HasActuator = false);
}