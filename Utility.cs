using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader.Config;
using System.Reflection;

namespace SPIC;

public readonly record struct NPCStats(int Total, int Boss);

public static class Utility {

    public class DescendingComparer<T> : IComparer<T> where T : System.IComparable<T> {
        public int Compare(T? x, T? y) {
            return y is null ? 1 : y.CompareTo(x);
        }
    }

    public static int CountItems(this Item[] container, int type, params int[] ignoreSots) {
        int total = 0;
        for (int i = 0; i < container.Length; i++) {
            if (System.Array.IndexOf(ignoreSots, i) == -1 && container[i].type == type)
                total += container[i].stack;
        }
        return total;
    }
    public static int CountItems(this Player player, int type, bool includeChest = false) {

        int total = player.inventory.CountItems(type, 58) + new Item[] { Main.mouseItem }.CountItems(type);
        if (includeChest && CrossMod.MagicStorageIntegration.Enabled && CrossMod.MagicStorageIntegration.InMagicStorage) total += CrossMod.MagicStorageIntegration.CountItems(type);
        if (includeChest && player.InChest(out Item[]? chest)) total += chest.CountItems(type);
        return total;
    }
    public static int CountItemsInWorld() {
        int i = 0;
        foreach (Item item in Main.item) if (!item.IsAir) i++;
        return i;
    }

    internal static int CountProjectilesInWorld() {
        int p = 0;
        foreach (Projectile proj in Main.projectile) if (proj.active) p++;
        return p;
    }


    public static bool InChest(this Player player, [MaybeNullWhen(false)] out Item[] chest) => (chest = player.Chest()) is not null;
    public static Item[]? Chest(this Player player) => player.chest switch {
        > -1 => Main.chest[player.chest].item,
        -2 => player.bank.item,
        -3 => player.bank2.item,
        -4 => player.bank3.item,
        -5 => player.bank4.item,
        _ => null
    };

    public static void RemoveFromInventory(this Player player, int type, int count = 1) {
        foreach (Item i in player.inventory) {
            if (i.type != type) continue;
            if (i.stack < count) {
                count -= i.stack;
                i.TurnToAir();
            } else {
                i.stack -= count;
                return;
            }
        }
    }

    public static Item? PickPaint(this Player player) {
        for (int i = 54; i < 58; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].paint > 0)
                return player.inventory[i];
        }
        for (int i = 0; i < 58; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].paint > 0)
                return player.inventory[i];
        }

        return null;
    }


    public static bool XMasDeco(this Item item) => Terraria.ID.ItemID.StarTopper1 <= item.type && item.type <= Terraria.ID.ItemID.BlueAndYellowLights;
    public static bool Placeable(this Item item) => item.XMasDeco() || item.createTile != -1 || item.createWall != -1;


    public static int WorldDifficulty() => Main.masterMode ? 2 : Main.expertMode ? 1 : 0;
    public static NPCStats GetNPCStats() {
        int total = 0;
        int boss = 0;
        foreach (NPC npc in Main.npc) {
            if (!npc.active) continue;
            total++;
            if (npc.boss) boss++;
        }
        NPCStats a = new(total, boss);
        return a;
    }

    public static void SaveConfig(this ModConfig config) => s_saveConfigMethod.Invoke(null, new object[]{config});
    internal static void LoadConfig(this ModConfig config) => s_loadConfigMethod.Invoke(null, new object[]{config});

    public static K[] Reverse<K, V>(this Dictionary<K, V> dictionary, V value) where K: notnull where V : notnull {
        List<K> reverse = new();
        foreach (KeyValuePair<K,V> kvp in dictionary) {
            if (value.Equals(kvp.Value)) reverse.Add(kvp.Key);
        }
        return reverse.ToArray();
    }
    public static K? FindKey<K, V>(this Dictionary<K, V> dictionary, System.Predicate<KeyValuePair<K, V>> pred) where K: notnull {
        foreach (KeyValuePair<K, V> kvp in dictionary) {
            if (pred(kvp)) return kvp.Key;
        }
        return default;
    }
    public static V? FindValue<K, V>(this Dictionary<K, V> dictionary, System.Predicate<KeyValuePair<K, V>> pred) where K: notnull {
        foreach (var kvp in dictionary) {
            if (pred(kvp)) return kvp.Value;
        }
        return default;
    }

    public static object Index(this ICollection collection, int index){
        int i = 0;
        foreach(object o in collection){
            if(i == index) return o;
            i++;
        }
        throw new System.IndexOutOfRangeException("The index was outside the bounds of the array");
    }
    
    public static void Move(this IOrderedDictionary dict, int origIndex, int destIndex)
        => dict.Move(dict.Keys.Index(origIndex), destIndex);
    public static void Move(this IOrderedDictionary dict, object key, int destIndex){
        object? value = dict[key];
        dict.Remove(key);
        dict.Insert(destIndex, key, value);
    }
    
    public static bool TryAdd(this IOrderedDictionary dict, object key, object value){
        if(dict.Contains(key)) return false;
        dict.Add(key, value);
        return true;
    }


    public static bool ImplementsInterface(this System.Type type, System.Type iType, [MaybeNullWhen(false)] out System.Type impl)
        => (impl = System.Array.Find(type.GetInterfaces(), i => iType.IsGenericType ? i.IsGenericType && i.GetGenericTypeDefinition() == iType : iType == i)) != null;
    
    private readonly static MethodInfo s_saveConfigMethod = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic, new System.Type[] { typeof(ModConfig) })!;
    private readonly static MethodInfo s_loadConfigMethod = typeof(ConfigManager).GetMethod("Load", BindingFlags.Static | BindingFlags.NonPublic, new System.Type[] { typeof(ModConfig) })!;
}
