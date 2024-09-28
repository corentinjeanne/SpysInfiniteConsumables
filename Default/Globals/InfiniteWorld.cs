using System;
using System.Collections.Generic;
using System.Linq;
using SPIC.Default.Infinities;
using SpikysLib.Collections;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SPIC.Default.Globals;

[Flags]
public enum TileType : byte {
    Block = 0b01,
    Wall  = 0b10,
}

[Flags]
public enum WireType : byte {
    Red      = 0b00001,
    Blue     = 0b00010,
    Green    = 0b00100,
    Yellow   = 0b01000,
    Actuator = 0b10000,
}

public class InfiniteWorld : ModSystem {
    public static InfiniteWorld Instance => ModContent.GetInstance<InfiniteWorld>();
   
    public Player? contextPlayer = null;
    public Projectile? contextProjectile = null;

    public bool IsInfinitePlacementContext() {
        if (contextPlayer is not null) {
            Item item = contextPlayer.HeldItem;
            if (Main.LocalPlayer.HasInfinite(Placeable.GetAmmo(contextPlayer, item) ?? item, 1, Placeable.Instance)) return true;
        } else if (contextProjectile is not null) {
            if (contextProjectile.noDropItem) return true;
        }
        return false;
    }

    public void SetInfinite(int x, int y, TileType flags) => SetInfinite(x, y, _infiniteTiles, (int)flags, 2);
    public void SetInfinite(int x, int y, WireType flags) => SetInfinite(x, y, _infiniteWires, (int)flags, 8);
    private static void SetInfinite(int x, int y, Dictionary<int, int[]> data, int flags, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        data.GetOrAdd(chunkId, new int[ChunkSize * ChunkSize * bits / BitsPerIndex])[i] |= flags << offset;
    }
    public bool IsInfinite(int x, int y, TileType flags) => IsInfinite(x, y, _infiniteTiles, (int)flags, 2);
    public bool IsInfinite(int x, int y, WireType flags) => IsInfinite(x, y, _infiniteWires, (int)flags, 8);
    private static bool IsInfinite(int x, int y, Dictionary<int, int[]> data, int value, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        return data.TryGetValue(chunkId, out int[]? chunk) && (chunk[i] & (value << offset)) != 0;
    }
    public void ClearInfinite(int x, int y, TileType flags) => ClearInfinite(x, y, _infiniteTiles, (int)flags, 2);
    public void ClearInfinite(int x, int y, WireType flags) => ClearInfinite(x, y, _infiniteWires, (int)flags, 8);
    private static void ClearInfinite(int x, int y, Dictionary<int, int[]> data, int value, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        if (data.TryGetValue(chunkId, out int[]? chunk)) chunk[i] &= ~(value << offset);
    }

    private static (int chunkId, int index, int offset) GetChunkIndex(int x, int y, int bits) {
        int cornerX = Math.DivRem(x, ChunkSize, out int i) * ChunkSize;
        int cornerY = Math.DivRem(y, ChunkSize, out int j) * ChunkSize;
        j = Math.DivRem(j * bits, BitsPerIndex, out int offset);
        // based on Tile.TileId calculation
        return (cornerY + cornerX * Main.tile.Height, j + i * 2 * ChunkSize / BitsPerIndex, offset);
    }

    public override void SaveWorldData(TagCompound tag) {
        tag[InfiniteTilesKey] = _infiniteTiles.Where(kvp => kvp.Value.Exist(i => i != 0))
            .Select(kvp => new TagCompound() { { "key", kvp.Key }, { "value", kvp.Value } }).ToArray();
        tag[InfiniteWiresKey] = _infiniteWires.Where(kvp => kvp.Value.Exist(i => i != 0))
            .Select(kvp => new TagCompound() { { "key", kvp.Key }, { "value", kvp.Value } }).ToArray();
    }
    public override void LoadWorldData(TagCompound tag) {
        if (tag.TryGet(InfiniteTilesKey, out TagCompound[] infiniteTiles))
            _infiniteTiles = new(infiniteTiles.Select(tag => new KeyValuePair<int, int[]>(tag.Get<int>("key"), tag.Get<int[]>("value"))));
        if (tag.TryGet(InfiniteWiresKey, out TagCompound[] infiniteWires))
            _infiniteWires = new(infiniteTiles.Select(tag => new KeyValuePair<int, int[]>(tag.Get<int>("key"), tag.Get<int[]>("value"))));
    }
    public override void OnWorldUnload() {
        _infiniteTiles.Clear();
        _infiniteWires.Clear();
    }

    private Dictionary<int, int[]> _infiniteTiles = [];
    private Dictionary<int, int[]> _infiniteWires = [];

    public const int BitsPerIndex = 32;
    public const int ChunkSize = 64;

    public const string InfiniteTilesKey = "infiniteTiles";
    public const string InfiniteWiresKey = "infiniteWires";
}
