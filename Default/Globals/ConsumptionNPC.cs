using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using SPIC.Default.Infinities;

namespace SPIC.Default.Globals;

public sealed class ConsumptionNPC : GlobalNPC {
    
    public override void OnSpawn(NPC npc, IEntitySource source) {
        if (!Configs.InfinitySettings.Instance.PreventItemDuplication || source is not EntitySource_Parent parent
                || parent.Entity is not Player player || !player.HasInfinite(npc.catchItem, 1, Usable.Instance))
            return;
        npc.SpawnedFromStatue = true;
    }
}
