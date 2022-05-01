using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace SPIC.Systems {

    public struct ChunkID {
        public int X, Y;
        public ChunkID(int x, int y) { X = x; Y = y; }
        public string Tag() => $"{X} {Y}";
        public int[] AsArray() => new int[] { X,Y };
    }
    public class Chunk {

        public readonly int Size;
        public readonly byte[] Tiles;
        public bool IsEmpty => System.Array.FindIndex(Tiles, b => b != 0) == -1;
        public Chunk(int size) {
            Size = size;
            Tiles = new byte[(Size+3)/4 * Size];
        }
        public Chunk(int size, byte[] tiles) {
            Size = size;
            Tiles = tiles;
        }

        public void SetBlock(int x, int y, bool val) {
            var (index, key) = IndexKeyPair(x, y);
            SetBit(index, key, val);
        }
        public bool GetBlock(int x, int y) {
            var (index, key) = IndexKeyPair(x, y);
            return GetBit(index, key);
        }
        public void SetWall(int x, int y, bool val) {
            var (index, key) = IndexKeyPair(x, y);
            SetBit(index, key+1, val);
        }
        public bool GetWall(int x, int y) {
            var (index, key) = IndexKeyPair(x, y);
            return GetBit(index, key+1);
        }
        private bool GetBit(int index, int key) => (Tiles[index] & (1 << key)) != 0;
        private void SetBit(int index, int key, bool value) {
            if (value) Tiles[index] |= (byte)(1 << key);
            else Tiles[index] &= (byte)~(1 << key);
        }
        private (int index, int key) IndexKeyPair(int x, int y) => ((x + y * Size)/4, 2 * (x % 4));
             
    }
    public class SpicWorld : ModSystem {

        private int _chunkSize = 64;
        private readonly Dictionary<ChunkID, Chunk> _chunks = new();
        
        private const string TAG_CREATED = "Chunks";
        private const string TAG_SIZE = "Size";
        public void PlaceBlock(int i, int j) {
            GetChunk(ref i, ref j, canCreate: true).SetBlock(i, j, true);
        }
        public void PlaceWall(int i, int j) {
            GetChunk(ref i, ref j, canCreate: true).SetWall(i, j, true);
        }
        public bool MineBlock(int i, int j) {
            Chunk chunk = GetChunk(ref i, ref j);
            if (chunk == null) return false;

            var val = chunk.GetBlock(i, j);
            chunk.SetBlock(i, j, false);
            return val;
        }
        public bool MineWall(int i, int j) {
            Chunk chunk = GetChunk(ref i, ref j);
            if (chunk == null) return false;

            var val = chunk.GetWall(i, j);
            chunk.SetWall(i, j, false);
            return val;
        }
        public Chunk GetChunk(ref int i, ref int j, bool canCreate = false) {
            int x = i / _chunkSize;
            int y = j / _chunkSize;
            i %= _chunkSize; j %= _chunkSize;

            ChunkID id = new(x, y);
            if (!_chunks.ContainsKey(id)) {
                if (!canCreate) return null;
                _chunks.Add(id, new Chunk(_chunkSize));
            }
            return _chunks[id];

        }


        public override void LoadWorldData(TagCompound tag) {
            _chunks.Clear();
            _chunkSize = tag.GetInt(TAG_SIZE);
            List<ChunkID> createdChunks = (tag[TAG_CREATED] as List<int[]>).ConvertAll(ia => new ChunkID(ia[0], ia[1]));
            foreach (ChunkID id in createdChunks) {
                _chunks.Add(id, new Chunk(_chunkSize, tag.GetByteArray(id.Tag())));
            }
        }

        public override void SaveWorldData(TagCompound tag) {

            Configs.ConsumableConfig.Instance.ManualSave();

            if (_chunks.Count != 0) {
                tag.Add(TAG_SIZE, _chunkSize);
                List<int[]> createdChunks = new();
                foreach ((ChunkID id, Chunk chunck) in _chunks) {
                    if (chunck.IsEmpty) continue;
                    createdChunks.Add(id.AsArray());
                    tag.Add(id.Tag(), chunck.Tiles);
                }
                tag.Add(TAG_CREATED, createdChunks);
            }
        }
        public override void OnWorldUnload() {
            _chunks.Clear();
        }

        public static int preUseDifficulty;
        public static int preUseInvasion;
        public static NPCStats preUseNPCStats;

        // May be moved into player class
        public static void SavePreUseItemStats() {
            preUseDifficulty = Utility.WorldDifficulty();
            preUseInvasion = Main.invasionType;
            preUseNPCStats = Utility.GetNPCStats();
        }
    }
}