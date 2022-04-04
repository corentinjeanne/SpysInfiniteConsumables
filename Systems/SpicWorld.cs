using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace SPIC.Systems {

	public struct ChunkID {
		public int X, Y;
		public ChunkID(int x, int y) { X = x; Y = y; }
		public override string ToString() => $"{X} {Y}";
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
			var i = IndexKeyPair(x, y);
			SetBit(i.index, i.key, val);
		}
		public bool GetBlock(int x, int y) {
			var i = IndexKeyPair(x, y);
			return GetBit(i.index, i.key);
		}
		public void SetWall(int x, int y, bool val) {
			var i = IndexKeyPair(x, y);
			SetBit(i.index, i.key+1, val);
		}
		public bool GetWall(int x, int y) {
			var i = IndexKeyPair(x, y);
			return GetBit(i.index, i.key+1);
		}
		private bool GetBit(int index, int key) => (Tiles[index] & (1 << key)) != 0;
		private void SetBit(int index, int key, bool value) {
			if (value) Tiles[index] |= (byte)(1 << key);
			else Tiles[index] &= (byte)~(1 << key);
		}
		private (int index, int key) IndexKeyPair(int x, int y) => ((x + y * Size)/4, 2 * (x % 4));
			 
	}
	public class SpicWorld : ModSystem {

		private int m_ChunkSize = 64;
		private readonly Dictionary<ChunkID, Chunk> m_Chunks = new();

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
			int x = i / m_ChunkSize;
			int y = j / m_ChunkSize;
			i %= m_ChunkSize; j %= m_ChunkSize;

			ChunkID id = new(x, y);
			if (!m_Chunks.ContainsKey(id)) {
				if (!canCreate) return null;
				m_Chunks.Add(id, new Chunk(m_ChunkSize));
			}
			return m_Chunks[id];

		}
		//public override void OnWorldLoad() {
		//	m_Chunks.Clear();
		//}

		public override void LoadWorldData(TagCompound tag) {
			m_Chunks.Clear();
			m_ChunkSize = tag.GetInt("Size");
			List<ChunkID> createdChunks = (tag["Chunks"] as List<int[]>).ConvertAll(ia => new ChunkID(ia[0], ia[1]));
			foreach (ChunkID id in createdChunks) {
				m_Chunks.Add(id, new Chunk(m_ChunkSize, tag.GetByteArray(id.ToString())));
			}
		}
		public override void SaveWorldData(TagCompound tag) {

			Configs.ConsumableConfig.Instance.ManualSave();

			if (m_Chunks.Count != 0) {
				tag.Add("Size", m_ChunkSize);
				List<int[]> createdChunks = new();
				foreach ((ChunkID id, Chunk chunck) in m_Chunks) {
					if (chunck.IsEmpty) continue;
					createdChunks.Add(id.AsArray());
					tag.Add(id.ToString(), chunck.Tiles);
				}
				tag.Add("Chunks", createdChunks);
			}
		}
		public override void OnWorldUnload() {
			m_Chunks.Clear();
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