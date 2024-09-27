using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SPIC.Default.Globals;

[Flags]
public enum TileType : byte {
    Block = 0b01,
    Wall = 0b10,
}

// TODO save infinite tiles
public class InfiniteWorld : ModSystem {
    public static InfiniteWorld Instance => ModContent.GetInstance<InfiniteWorld>();

    public void SetInfinite(int x, int y, TileType type) => _infiniteTiles.Add((x, y, type));
    public bool IsInfinite(int x, int y, TileType type) => _infiniteTiles.Contains((x, y, type));
    public void ClearInfinite(int x, int y, TileType type) => _infiniteTiles.Remove((x, y, type));

    public override void LoadWorldData(TagCompound tag) { }
    public override void SaveWorldData(TagCompound tag) { }
    public override void OnWorldUnload() => _infiniteTiles.Clear();

    private readonly HashSet<(int x, int y, TileType)> _infiniteTiles = [];
}
