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
public enum TileFlags : byte {
    Block = 0b01,
    Wall  = 0b10,
    All   = 0b11,
}

[Flags]
public enum WireFlags : byte {
    Red      = 0b00001,
    Blue     = 0b00010,
    Green    = 0b00100,
    Yellow   = 0b01000,
    Actuator = 0b10000,
    All      = 0b11111,
}

public class InfiniteWorld : ModSystem {
    public static InfiniteWorld Instance => ModContent.GetInstance<InfiniteWorld>();

    public Player? contextPlayer;
    public Projectile? contextProjectile;

    public bool IsInfinitePlacementContext() {
        SpysInfiniteConsumables.Instance.Logger.Debug($"Context check (netMode={Main.netMode})");
        if (contextPlayer is not null) {
            Item item = contextPlayer.HeldItem;
            if (contextPlayer.HasInfinite(Placeable.GetAmmo(contextPlayer, item) ?? item, 1, Placeable.Instance)) return true;
        } else if (contextProjectile is not null) {
            if (contextProjectile.noDropItem || contextProjectile.GetGlobalProjectile<ExplosionProjectile>().infiniteFallingTile) return true;
        }
        return false;
    }

    public void SetInfinite(int x, int y, TileFlags flags) => SetInfinite(x, y, (int)flags, _infiniteTiles, TileBits);
    public void SetInfinite(int x, int y, WireFlags flags) => SetInfinite(x, y, (int)flags, _infiniteWires, WireBits);
    private static void SetInfinite(int x, int y, int flags, Dictionary<int, int[]> data, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        data.GetOrAdd(chunkId, new int[ChunkSize * ChunkSize * bits / BitsPerIndex])[i] |= flags << offset;
    }
    public bool IsInfinite(int x, int y, TileFlags flags) => GetInfinite(x, y, flags) != 0;
    public bool IsInfinite(int x, int y, WireFlags flags) => GetInfinite(x, y, flags) != 0;
    public TileFlags GetInfinite(int x, int y, TileFlags flags = TileFlags.All) => (TileFlags)GetInfinite(x, y, (int)flags, _infiniteTiles, TileBits);
    public WireFlags GetInfinite(int x, int y, WireFlags flags = WireFlags.All) => (WireFlags)GetInfinite(x, y, (int)flags, _infiniteWires, WireBits);
    private static int GetInfinite(int x, int y, int flags, Dictionary<int, int[]> data, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        return data.TryGetValue(chunkId, out int[]? chunk) ? (chunk[i] >> offset) & flags : 0;
    }

    public void ClearInfinite(int x, int y, TileFlags flags) => ClearInfinite(x, y, (int)flags, _infiniteTiles, TileBits);
    public void ClearInfinite(int x, int y, WireFlags flags) => ClearInfinite(x, y, (int)flags, _infiniteWires, WireBits);
    private static void ClearInfinite(int x, int y, int flags, Dictionary<int, int[]> data, int bits) {
        (int chunkId, int i, int offset) = GetChunkIndex(x, y, bits);
        if (data.TryGetValue(chunkId, out int[]? chunk)) chunk[i] &= ~(flags << offset);
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

    public const int TileBits = 2;
    public const int WireBits = 8;

    private Dictionary<int, int[]> _infiniteTiles = [];
    private Dictionary<int, int[]> _infiniteWires = [];

    public const int BitsPerIndex = 32;
    public const int ChunkSize = 64;

    public const string InfiniteTilesKey = "infiniteTiles";
    public const string InfiniteWiresKey = "infiniteWires";
}
