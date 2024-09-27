using System;
using System.Collections.Generic;
using System.Linq;
using SpikysLib.Collections;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SPIC.Default.Globals;

[Flags]
public enum TileType : byte {
    Block = 0b01,
    Wall = 0b10,
}

public class InfiniteWorld : ModSystem {
    public static InfiniteWorld Instance => ModContent.GetInstance<InfiniteWorld>();

    public void SetInfinite(int x, int y, TileType type) {
        (int chunkID, int i, int offset) = GetChunkIndex(x, y, 2);
        _infiniteTiles.GetOrAdd(chunkID, new int[ChunkSize * ChunkSize / BitsPerIndex * 2])[i] |= ((byte)type) << offset;
    }
    public bool IsInfinite(int x, int y, TileType type) {
        (int chunkID, int i, int offset) = GetChunkIndex(x, y, 2);
        if (!_infiniteTiles.TryGetValue(chunkID, out int[]? chunk)) return false;
        return (chunk[i] & (byte)type << offset) != 0;
    }
    public bool ClearInfinite(int x, int y, TileType type) {
        (int chunkID, int i, int offset) = GetChunkIndex(x, y, 2);
        if (!_infiniteTiles.TryGetValue(chunkID, out int[]? chunk)) return false;
        bool value = (chunk[i] & ((byte)type << offset)) != 0;
        chunk[i] &= ~((byte)type << offset);
        return value;
    }

    private static (int chunkID, int index, int offset) GetChunkIndex(int x, int y, int bits) {
        int cornerX = Math.DivRem(x, ChunkSize, out int i) * ChunkSize;
        int cornerY = Math.DivRem(y, ChunkSize, out int j) * ChunkSize;
        j = Math.DivRem(j * bits, BitsPerIndex, out int offset);
        // based on Tile.TileId calculation
        return (cornerY + cornerX * Main.tile.Height, j + i * 2 * ChunkSize / BitsPerIndex, offset);
    }

    public override void SaveWorldData(TagCompound tag) {
        tag[InfiniteTilesKey] = _infiniteTiles.Where(kvp => kvp.Value.Exist(i => i != 0))
            .Select(kvp => new TagCompound() { { "key", kvp.Key }, { "value", kvp.Value } }).ToArray();
    }
    public override void LoadWorldData(TagCompound tag) {
        if (tag.TryGet(InfiniteTilesKey, out TagCompound[] infiniteTiles))
            _infiniteTiles = new(infiniteTiles.Select(tag => new KeyValuePair<int, int[]>(tag.Get<int>("key"), tag.Get<int[]>("value"))));
    }
    public override void OnWorldUnload() => _infiniteTiles.Clear();


    private Dictionary<int, int[]> _infiniteTiles = [];

    public const int BitsPerIndex = 32;
    public const int ChunkSize = 64;

    public const string InfiniteTilesKey = "infiniteTiles";
}
