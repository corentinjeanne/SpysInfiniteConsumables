using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace SPIC.Systems {


	public class BlockChunk {

		public readonly byte[] Tiles;
		public readonly Point Size;
		public BlockChunk(int size) {
			Size = new Point(size, size);
			Tiles = new byte[GetSize(Size.X, Size.Y)];
		}
		public BlockChunk (int width, int height) {
			Size = new Point(width, height);
			Tiles = new byte[GetSize(Size.X, Size.Y)];
		}
		public BlockChunk(int width, int height, byte[] data) {
			Size = new Point(width, height);
			Tiles = data;
		}
		public static int GetSize(int X, int Y) => (X * Y * 2 + 7) / 8;
		public void SetTile(int x, int y, bool val) {
			var i = IndexKeyPair(x, y);
			SetBit(i.index, i.key, val);
		}
		public bool GetTile(int x, int y) {
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
		private (int index, int key) IndexKeyPair(int x, int y) {

			return ((x + y * Size.X)/4, 2 * (x % 4));
		}
			 
	}
	public class SpicWorld : ModSystem {

		public static int preUseDifficulty;
		public static int preUseInvasion;
		public static NPCStats preUseNPCStats;
		private string m_ChunkFilePath;


		private List<int> m_CreatedChunks = new();
		private int m_ChunkSize = 128;
		public const int MAX_LOADED_CHUNKS = 16;
		private List<(int id, BlockChunk chunk)> m_LoadedChunks = new();

		public void PlaceTile(int i, int j) {
			// Check if chunk is Loaded
			int chunkIndex = LoadChunk(GetChunkId(ref i,ref j));

			// set the value
			m_LoadedChunks[chunkIndex].chunk.SetTile(i, j, true);
		}
		public void PlaceWall(int i, int j) {
			// Check if chunk is Loaded
			int chunkIndex = LoadChunk(GetChunkId(ref i, ref j));

			// set the value
			m_LoadedChunks[chunkIndex].chunk.SetWall(i, j, true);
		}
		public bool MineTile(int i, int j) {
			// Check if chunk is Loaded
			int chunkIndex = LoadChunk(GetChunkId(ref i, ref j), false);
			if(chunkIndex == -1) return false;
			// get the value
			bool val = m_LoadedChunks[chunkIndex].chunk.GetTile(i, j);
			m_LoadedChunks[chunkIndex].chunk.SetTile(i, j, false);
			return val;
		}
		public bool MineWall(int i, int j) {
			// Check if chunk is Loaded
			int chunkIndex = LoadChunk(GetChunkId(ref i, ref j), false);
			if (chunkIndex == -1) return false;
			// get the value
			bool val = m_LoadedChunks[chunkIndex].chunk.GetWall(i, j);
			m_LoadedChunks[chunkIndex].chunk.SetWall(i, j, false);
			return val;
		}


		public override void OnWorldLoad() {
			m_ChunkFilePath = Main.worldPathName.Replace(".wld", ".spic");
		}
		public override void LoadWorldData(TagCompound tag) {
			

			if (Main.netMode != NetmodeID.SinglePlayer) return;

			long lenght = -1;
			if (File.Exists(m_ChunkFilePath)) {
				m_CreatedChunks = tag["chunks"] as List<int>;
				m_ChunkSize = (int)tag["chunksSize"];
				using (FileStream fileStream = new FileStream(m_ChunkFilePath, FileMode.Open, FileAccess.Read)) {
					lenght = fileStream.Length;
				}
				if (m_CreatedChunks.Count != lenght / BlockChunk.GetSize(m_ChunkSize, m_ChunkSize)) {
					Mod.Logger.Error("Mishmash between world data and .spic file data");
					m_CreatedChunks.Clear();
					File.Move(m_ChunkFilePath, m_ChunkFilePath+".bad", true);
				}
			}else {
				Mod.Logger.Error("World has spic data but not file was found");
			}
		}
		public override void SaveWorldData(TagCompound tag) {

			Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

			config.ManualSave();


			if (m_CreatedChunks.Count > 0) {
				tag["chunks"] = m_CreatedChunks;
				tag["chunksSize"] = m_ChunkSize;
			}
		}
		public override void OnWorldUnload() {
			for (int i = 0; i < MAX_LOADED_CHUNKS; i++) UnLoadChunk(i);
			m_LoadedChunks.Clear();
			m_CreatedChunks.Clear();
		}
		public int FileOffset(int chuckID) {
			int chunkNumber = m_CreatedChunks.IndexOf(chuckID);
			if (chunkNumber < 0) throw new System.IndexOutOfRangeException();

			return chunkNumber * BlockChunk.GetSize(m_ChunkSize, m_ChunkSize);
		}

		/// <summary>
		/// Return the id of the chunk in with is the block (i,j)
		/// Clamps i and j in the chunk
		/// </summary>
		public int GetChunkId(ref int i, ref int j) {
			int chunkX = i / m_ChunkSize;
			int chunkY = j / m_ChunkSize;
			i %= m_ChunkSize; j %= m_ChunkSize;
			return chunkX + chunkY * (Main.tile.Width+m_ChunkSize - 1)/ m_ChunkSize;
		}
		public int GetChunkId(int i, int j) {
			return GetChunkId(ref i, ref j);
		}

		public int LoadChunk(int id, bool createNew = true) {

			// Check if chunk is already loaded
			for (int i = 0; i < m_LoadedChunks.Count; i++) {
				if(m_LoadedChunks[i].id == id) return i;
			}

			// Unload Chucks if to many are loaded
			if (m_LoadedChunks.Count >= MAX_LOADED_CHUNKS) UnLoadChunk(0);

			// Create the File if if doesn't exist
			if(m_CreatedChunks.Count != 0) {
				if (!createNew) return -1;
				File.Create(m_ChunkFilePath).Close();
			}

			BlockChunk toLoad;
			// Creates the chunk it it doesn't exist
			if (!m_CreatedChunks.Contains(id)) {

				if (!createNew) return -1;
				
				// Creates the chunk
				toLoad = new BlockChunk(m_ChunkSize, m_ChunkSize);

				// Adds the empty chunk to the file
				using FileStream stream = new FileStream(m_ChunkFilePath, FileMode.Append);
				stream.Write(toLoad.Tiles, 0, toLoad.Tiles.Length);
				Mod.Logger.Debug($"Created chunk {id}");
				m_CreatedChunks.Add(id);
				// Loads the chunk
			}
			else {

				// Reads the data from the file
				byte[] data = new byte[BlockChunk.GetSize(m_ChunkSize, m_ChunkSize)];
				using FileStream stream = new FileStream(m_ChunkFilePath, FileMode.Open);
				stream.Seek(FileOffset(id), SeekOrigin.Begin);
				stream.Read(data, 0, data.Length);

				// Sets the data to the chunk
				toLoad = new BlockChunk(m_ChunkSize, m_ChunkSize, data);
			}
			// Loads the chunk from the file
			m_LoadedChunks.Add((id, toLoad));
			return m_LoadedChunks.Count - 1;


		}
		public void UnLoadChunk(int index) {
			if(index >= m_LoadedChunks.Count) return;

			// Writes the chunk to the file
			using FileStream stream = new FileStream(m_ChunkFilePath, FileMode.Open);
			stream.Seek(FileOffset(m_LoadedChunks[index].id), SeekOrigin.Begin);
			stream.Write(m_LoadedChunks[index].chunk.Tiles, 0, m_LoadedChunks[index].chunk.Tiles.Length);

			// Unload the chunk
			m_LoadedChunks.RemoveAt(index);
		}

		// May be moved into player class
		public static void SavePreUseItemStats() {
			preUseDifficulty = Utility.WorldDifficulty();
			preUseInvasion = Main.invasionType;
			preUseNPCStats = Utility.GetNPCStats();
		}
	}
}