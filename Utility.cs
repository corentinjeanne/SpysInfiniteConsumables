using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SPIC;

public static class Utility {

    public static int NameToType(string name, bool noCaps = true) {
        string fullName = name.Replace("_", " ");
        if (noCaps) fullName = fullName.ToLower();

        for (int k = 0; k < ItemLoader.ItemCount; k++) {
            string testedName = noCaps ? Lang.GetItemNameValue(k).ToLower() : Lang.GetItemNameValue(k);
            if (fullName == testedName) return k;
        }

        throw new UsageException("Invalid Name" + name);
    }

    public static bool InChest(this Player player, [MaybeNullWhen(false)] out Item[] chest) => (chest = player.Chest()) is not null;
    public static Item[]? Chest(this Player player) => player.chest switch {
        > -1 => Main.chest[player.chest].item,
        -2 => player.bank.item,
        -3 => player.bank2.item,
        -4 => player.bank3.item,
        -6 => player.bank4.item,
        _ => null
    };

    public static int CountItems(this Item[] container, int type, params int[] ignoreSots) {
        int total = 0;
        for (int i = 0; i < container.Length; i++) {
            if (System.Array.IndexOf(ignoreSots, i) == -1 && container[i].type == type)
                total += container[i].stack;
        }
        return total;
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

    public static int CountItems(this Player player, int type, bool includeChest = false) {

        int total = player.inventory.CountItems(type, 58) + new Item[] { Main.mouseItem }.CountItems(type);
        if (includeChest && CrossMod.MagicStorageIntegration.Enable && CrossMod.MagicStorageIntegration.InMagicStorage) total += CrossMod.MagicStorageIntegration.CountItems(type);
        if (includeChest && player.InChest(out Item[]? chest)) total += chest.CountItems(type);
        return total;
    }

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

    public static long CountCoins(this Item[] container, params int[] ignoreSlots) {
        long count = 0L;
        for (int i = 0; i < container.Length; i++) {
            if (System.Array.IndexOf(ignoreSlots, i) == -1 && container[i].IsACoin)
                count += (long)container[i].value / 5 * container[i].stack;
        }
        return count;
    }

    public static bool Placeable(this Item item) => item.createTile != -1 || item.createWall != -1;

    public static int WorldDifficulty() => Main.masterMode ? 2 : Main.expertMode ? 1 : 0;

    public readonly record struct NPCStats(int Total, int Boss);

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

    public static int RequirementToItems(int infinity, int maxStack) => infinity switch {
        < 0 => -infinity * maxStack,
        _ => infinity
    };

    public static int CountItemsInWorld() {
        int i = 0;
        foreach (Item item in Main.item) if (!item.IsAir) i++;
        return i;
    }

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

    public static void SaveConfig(this ModConfig config) {
        using StreamWriter sw = new(ConfigManager.ModConfigPath + $"\\{config.Mod.Name}_{config.Name}.json");
        sw.Write(JsonConvert.SerializeObject(config, ConfigManager.serializerSettings));
    }

    public enum RangeOptions : byte {
        ExcludeAll = 0b00,
        IncludeMin = 0b01,
        IncludeMax = 0b10,
        IncludeAll = IncludeMin | IncludeMax
    }

    public static bool InRange<T>(this T value, T min, T max, RangeOptions options = RangeOptions.IncludeMin) where T : System.IComparable<T>
        => (options.HasFlag(RangeOptions.IncludeMin) ? min.CompareTo(value) <= 0 : min.CompareTo(value) < 0) &&
            (options.HasFlag(RangeOptions.IncludeMax) ? value.CompareTo(max) <= 0 : value.CompareTo(max) < 0);


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
    public static bool TryGet(IDictionary dict, object key, out object? value) {
        if (dict.Contains(key)) {
            value = dict[key];
            return true;
        }
        value = default;
        return false;
    }

    public static T? Find<T>(this IEnumerable<T> collection, System.Predicate<T> predicate) {
        foreach (T v in collection) {
            if (predicate(v)) return v;
        }
        return default;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, T value) {
        int i = 0;
        foreach (T v in collection) {
            if (value is null ? v is null : value.Equals(v)) return i;
            i++;
        }
        return -1;
    }
}
