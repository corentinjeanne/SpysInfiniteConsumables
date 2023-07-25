using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
namespace SPIC.CrossMod;

[JITWhenModsEnabled(ModName)]
public static class MagicStorageIntegration {
    public const string ModName = "MagicStorage";

    public static bool Enabled => ModLoader.HasMod(ModName);
    public static bool InMagicStorage => Main.worldID != 0 && Main.LocalPlayer?.GetModPlayer<MagicStorage.StoragePlayer>().GetStorageHeart() is not null;
    public static System.Version Version => ModLoader.GetMod(ModName).Version;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CountItems(int type, int? prefix = null) {
        int count = 0;
        foreach (Item i in MagicStorage.StoragePlayer.LocalPlayer.GetStorageHeart().GetStoredItems())
            if (i.type == type && (!prefix.HasValue || i.prefix == prefix)) count += i.stack;
        return count;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool Countains(Item item) {
        if (!InMagicStorage) return false;
        foreach (Item i in MagicStorage.StoragePlayer.LocalPlayer.GetStorageHeart().GetStoredItems())
            if (i.type == item.type && i.prefix == item.prefix) return true;
        return false;
    }
}