using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria.ModLoader.Config.UI;
using SpikysLib.Extensions;
using SpikysLib.CrossMod;

namespace SPIC;

public readonly record struct NPCStats(int Total, int Boss);

public static class Utility {

    public static int CountItemsInWorld() {
        int i = 0;
        foreach (Item item in Main.item) if (!item.IsAir) i++;
        return i;
    }

    public static int CountProjectilesInWorld() {
        int p = 0;
        foreach (Projectile proj in Main.projectile) if (proj.active) p++;
        return p;
    }

    public static bool IsSimilar(this Item a, Item b, bool loose = true) => loose ? !a.IsNotSameTypePrefixAndStack(b) : a == b;

    public static bool IsFromVisibleInventory(this Player player, Item item, bool loose = true) {
        if (Main.mouseItem.IsSimilar(item, loose)
                || Array.Find(player.inventory, i => i.IsSimilar(item, loose)) is not null
                || (player.InChest(out var chest) && Array.Find(chest, i => i.IsSimilar(item, loose)) is not null)
                || (MagicStorageIntegration.Enabled && MagicStorageIntegration.Countains(item)))
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

    public static void PortConfig(this ModConfig config) {
        config.Load();
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
            (PropertyFieldWrapper wrapper, obj) = host.GetMember(obj, movedTo.Members);
            wrapper.SetValue(obj, value);
            DefaultValueAttribute? defaultValue = oldField.GetCustomAttribute<DefaultValueAttribute>();
            oldField.SetValue(config, defaultValue?.Value ?? (wrapper.Type.IsValueType ? Activator.CreateInstance(wrapper.Type) : null));
        }
        config.Save();
    }

    public static void SetConfig(object instance, object? config) => instance.GetType().GetField("Config", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public)?.SetValue(null, config);
}
