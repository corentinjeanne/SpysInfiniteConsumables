using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using SPIC.Config;

namespace SPIC.Systems {

    public class SpicWorld : ModSystem {

        public static int preUseDifficulty;
        public static int preUseInvasion;
        public static int preUseBossCount;

        private bool m_Modified;
        private string NoDropPath;
        private byte[] noDropBloks;
        private int m_Length, m_Height;

        public void PlaceTile(int i, int j) => SetBit(i / 4 + j * m_Length, 2 * (i % 4), true);
        
        public void PlaceWall(int i, int j) => SetBit(i / 4 + j * m_Length, 2 * (i % 4) + 1, true);
        public bool MineTile(int i, int j) {
            int index = i / 4 + j * m_Length;
            int key = 2 * (i % 4);
            bool value = GetBit(index, key);
            SetBit(index, key, false);
            return value;
        }
        public bool MineWall(int i, int j) {
            int index = i / 4 + j * m_Length;
            int key = 2 * (i % 4) + 1;
            bool value = GetBit(index, key);
            SetBit(index, key, false);
            return value;
        }

        private bool GetBit(int index, int offset) => (noDropBloks[index] & (1 << offset)) != 0;
        private void SetBit(int index, int offset, bool value) {
            m_Modified = true;
            if (value) noDropBloks[index] |= (byte)(1 << offset);
            else       noDropBloks[index] &= (byte)~(1 << offset);
        }

		public override void OnWorldLoad() {
            // Does not load the world file in multi
            if (Main.netMode != NetmodeID.SinglePlayer) return;

            m_Length = (Main.ActiveWorldFileData.WorldSizeX*2 + 7) / 8;
            m_Height = Main.ActiveWorldFileData.WorldSizeY;
            NoDropPath = Main.worldPathName.Replace(".wld", ".spic");

            noDropBloks = File.Exists(NoDropPath) ? File.ReadAllBytes(NoDropPath) : new byte[m_Length * m_Height];

        }
		public override void Unload() {
            noDropBloks = null;
		}
		public override void SaveWorldData(TagCompound tag) {
            ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

            if (config.modifiedInGame)
                config.ManualSave();

            // Does not save the world file in multi
            if (Main.netMode != NetmodeID.SinglePlayer) return;
            if (m_Modified) {
                File.WriteAllBytes(NoDropPath, noDropBloks);
                m_Modified = false;
            }
        }
        public static void PreUseItem() {
            preUseDifficulty = Utility.WorldDifficulty;
            preUseInvasion = Main.invasionType;
            preUseBossCount = Utility.BossCount();
		}
    }
}