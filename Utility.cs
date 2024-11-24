using Terraria;
using Terraria.ID;
using System.Text.RegularExpressions;
using Terraria.UI;

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

    public static bool XMasDeco(this Item item) => ItemID.StarTopper1 <= item.type && item.type <= ItemID.BlueAndYellowLights;
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

    public static string PrettyPrintName(string name) => Regex.Replace(name, "([A-Z])", " $1").Trim();
}
