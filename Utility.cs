using Terraria;
using Terraria.ModLoader;

namespace SPIC
{

    public struct NPCStats
    {
        public int boss, total;
    }
    public static class Utility
    {

        public static int WorldDifficulty()
        {
            return Main.masterMode ? 2 : Main.expertMode ? 1 : 0;
        }

        public static int NameToType(string name, bool noCaps = true)
        {
            string fullName = name.Replace("_", " ");
            if (noCaps)
            {
                fullName = fullName.ToLower();
            }

            for (int k = 0; k < ItemLoader.ItemCount; k++)
            {
                string testedName = noCaps ? Lang.GetItemNameValue(k).ToLower() : Lang.GetItemNameValue(k);
                if (fullName == testedName)
                {
                    return k;
                }
            }
            throw new UsageException("Invalid Name" + name);
        }
        public static Item[] Chest(this Player player)
        {
            return player.chest switch
            {
                > -1 => Main.chest[player.chest].item,
                -2 => player.bank.item,
                -3 => player.bank2.item,
                -4 => player.bank3.item,
                -6 => player.bank4.item,
                _ => null
            };
        }

        public static int CountInContainer(Item[] container, int type)
        {
            int total = 0;
            foreach (Item i in container)
            {
                if (i.type == type)
                {
                    total += i.stack;
                }
            }
            return total;
        }
        public static void RemoveFromInventory(this Player player, int type, int count = 1)
        {
            foreach (Item i in player.inventory)
            {
                if (i.type != type)
                {
                    continue;
                }

                if (i.stack > count)
                {
                    i.stack -= count;
                    return;
                }
                count -= i.stack;
                i.TurnToAir();
            }
        }
        public static int CountAllItems(this Player player, int type, bool includeChest = false)
        {
            int total = CountInContainer(player.inventory, type);
            Item[] chest;
            if (includeChest && (chest = player.Chest()) is not null)
            {
                total += CountInContainer(chest, type);
            }

            return total;
        }

        public static NPCStats GetNPCStats()
        {
            NPCStats stats = new();
            foreach (NPC npc in Main.npc)
            {
                if (npc.active)
                {
                    stats.total++;
                    if (npc.boss)
                    {
                        stats.boss++;
                    }
                }
            }
            return stats;
        }

        public static int RequirementToItems(int infinity, int type, int theoricalMaxStack = 999)
        {
            int maxStack = Globals.SpicItem.MaxStack(type);
            return infinity >= 0 ? System.Math.Min(maxStack, infinity) :
                -infinity * System.Math.Min(maxStack, theoricalMaxStack);
        }

        public static bool Placeable(this Item item)
        {
            return item.createTile != -1 || item.createWall != -1;
        }
    }
}