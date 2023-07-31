using Terraria;
using Terraria.ModLoader;
using SPIC.Infinities;
using Terraria.DataStructures;

namespace SPIC.Globals;

public sealed class ConsumptionNPC : GlobalNPC {
    
    public override void OnSpawn(NPC npc, IEntitySource source) {
        if(source is EntitySource_Parent parent && Configs.InfinitySettings.Instance.PreventItemDuplication
                && parent.Entity is Player player && player.HasInfinite(npc.catchItem, 1, Usable.Instance)) {
            npc.SpawnedFromStatue = true;
        }
    }
}
