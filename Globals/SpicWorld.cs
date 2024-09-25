using SpikysLib.Configs;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace SPIC.Globals;

public class SpicWorld : ModSystem {

    public override void LoadWorldData(TagCompound tag) { }

    public override void SaveWorldData(TagCompound tag) {
        Configs.InfinitySettings.Instance.Save();
    }
}
