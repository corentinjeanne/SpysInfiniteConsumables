using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ModLoader;
namespace SPIC.CrossMod;

[JITWhenModsEnabled(ModName)]
public static class MagicStorageIntegration {
    public const string ModName = "MagicStorage";
    public static bool Enable => ModLoader.HasMod(ModName);

    public static bool InMagicStorage => MagicStorage.StoragePlayer.LocalPlayer.GetStorageHeart() is not null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CountItems(int type) {

        int count = 0;
        var storedItems = MagicStorage.StoragePlayer.LocalPlayer.GetStorageHeart().GetStoredItems();
        foreach (Item i in storedItems) if (i.type == type) count += i.stack;
        return count;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool Countains(Item item, bool isACopy = false) {
        if (!InMagicStorage) return false;
        // if(isACopy){
        if(CountItems(item.type) == item.stack) return true;
        return false;
    }
}