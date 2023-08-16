using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria.ModLoader.Config.UI;

namespace SPIC;

public readonly record struct NPCStats(int Total, int Boss);

public static class Utility {

    public static int CountItems(this Item[] container, int type, params int[] ignoreSots) {
        int total = 0;
        for (int i = 0; i < container.Length; i++) {
            if (Array.IndexOf(ignoreSots, i) == -1 && container[i].type == type)
                total += container[i].stack;
        }
        return total;
    }
    public static int CountItems(this Player player, int type, bool includeChest = false) {
        int total = player.inventory.CountItems(type, 58) + new [] { Main.mouseItem }.CountItems(type);
        total += new[] { Main.mouseItem }.CountItems(type) + new[] { Main.CreativeMenu.GetItemByIndex(0) }.CountItems(type);
        if (includeChest) {
            if (CrossMod.MagicStorageIntegration.Enabled && CrossMod.MagicStorageIntegration.InMagicStorage) total += CrossMod.MagicStorageIntegration.CountItems(type);
            else if (player.InChest(out Item[]? chest)) total += chest.CountItems(type);
            if(player.chest != -5 && player.useVoidBag()) total += player.bank4.item.CountItems(type);
        }
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

    public static Item? FindItemRaw(this Player player, int type) {
        int num = player.FindItem(type);
        return num == -1 ? null : player.inventory[num];
    }
    public static Item? PickPaint(this Player player) {
        for (int i = 54; i < 58; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].paint > 0) return player.inventory[i];
        }
        for (int i = 0; i < 54; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].paint > 0) return player.inventory[i];
        }

        return null;
    }
    public static Item? PickBait(this Player player) {
        for (int i = 54; i < 58; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].bait > 0) return player.inventory[i];
        }
        for (int i = 0; i < 50; i++) {
            if (player.inventory[i].stack > 0 && player.inventory[i].bait > 0) return player.inventory[i];
        }

        return null;
    }

    public static bool IsSimilar(this Item a, Item b, bool loose = true) => loose ? !a.IsNotSameTypePrefixAndStack(b) : a == b;

    public static bool IsFromVisibleInventory(this Player player, Item item, bool loose = true) {
        
        if (Main.mouseItem.IsSimilar(item, loose)
                || Array.Find(player.inventory, i => i.IsSimilar(item, loose)) is not null
                || (player.InChest(out var chest) && Array.Find(chest, i => i.IsSimilar(item, loose)) is not null)
                || (CrossMod.MagicStorageIntegration.Enabled && CrossMod.MagicStorageIntegration.Countains(item)))
            return true;
        return false;
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

    internal static void PortConfig(this ModConfig config) {
        config.LoadConfig();
        foreach(FieldInfo oldField in config.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            Configs.MovedTo? movedTo = CustomAttributeExtensions.GetCustomAttribute<Configs.MovedTo>(oldField);
            if (movedTo is null) continue;
            
            Type? host = movedTo.Host;
            object? obj = null;
            if (host is null) {
                host = config.GetType();
                obj = config;
            }
            object? value = oldField.GetValue(config);
            (PropertyFieldWrapper wrapper, obj) = GetMember(host, obj, movedTo.Members);
            wrapper.SetValue(obj, value);
            DefaultValueAttribute? defaultValue = oldField.GetCustomAttribute<DefaultValueAttribute>();
            oldField.SetValue(config, defaultValue?.Value ?? (wrapper.Type.IsValueType ? Activator.CreateInstance(wrapper.Type) : null));
        }
        config.SaveConfig();
    }

    public static object Index(this ICollection collection, int index){
        int i = 0;
        foreach(object o in collection){
            if(i == index) return o;
            i++;
        }
        throw new IndexOutOfRangeException("The index was outside the bounds of the array");
    }

    public static bool TryAdd(this IOrderedDictionary dict, object key, object value) {
        if (dict.Contains(key)) return false;
        dict.Add(key, value);
        return true;
    }
    public static void Move(this IOrderedDictionary dict, int origIndex, int destIndex) => dict.Move(dict.Keys.Index(origIndex), destIndex);
    public static void Move(this IOrderedDictionary dict, object key, int destIndex){
        object? value = dict[key];
        dict.Remove(key);
        dict.Insert(destIndex, key, value);
    }

    public static IEnumerable<(object key, object? value)> Items(this IDictionary dict) => dict.Items<object, object?>();
    public static IEnumerable<(Tkey key, TValue? value)> Items<Tkey, TValue>(this IDictionary dict) where Tkey : notnull {
        foreach(DictionaryEntry entry in dict) {
            yield return new((Tkey)entry.Key, (TValue?)entry.Value);
        }
    }


    public static bool ImplementsInterface(this Type type, Type iType, [NotNullWhen(true)] out Type? impl)
        => (impl = Array.Find(type.GetInterfaces(), i => iType.IsGenericType ? i.IsGenericType && i.GetGenericTypeDefinition() == iType : iType == i)) != null;
    public static bool IsSubclassOfGeneric(this Type? type, Type generic, [NotNullWhen(true)] out Type? impl) {
        while (type != null && type != typeof(object)) {
            Type cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (generic == cur) {
                impl = type;
                return true;
            }
            type = type.BaseType;
        }
        impl = null;
        return false;
    }


    public static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);

    internal static (PropertyFieldWrapper, object?) GetMember(Type host, object? obj, Span<string> members) {
        MemberInfo member = host.GetMember(members[0], BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)[0];
        if (member is PropertyInfo property) {
            if (members.Length == 1) return (new(property), obj);
            obj = property.GetValue(obj);
            host = property.PropertyType;
        } else if (member is FieldInfo field) {
            if (members.Length == 1) return (new(field), obj);
            obj = field.GetValue(obj);
            host = field.FieldType;
        }
        return GetMember(host, obj, members[1..]);
    }

    private readonly static MethodInfo s_saveConfigMethod = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(ModConfig) })!;
    private readonly static MethodInfo s_loadConfigMethod = typeof(ConfigManager).GetMethod("Load", BindingFlags.Static | BindingFlags.NonPublic, new[] { typeof(ModConfig) })!;
}
