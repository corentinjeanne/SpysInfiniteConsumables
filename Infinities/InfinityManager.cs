using System.Collections.Generic;
using Terraria;

using SPIC.Infinities;

namespace SPIC;

internal sealed class InfinityCache {
    public readonly Dictionary<int, byte> categories = new();
    public readonly Dictionary<int, int> requirements = new();
    public readonly Dictionary<int, long> infinities = new();

    public void ClearAll(){
        categories.Clear();
        requirements.Clear();
        infinities.Clear();
    }
    public void ClearType(int type){
        categories.Remove(type);
        requirements.Remove(type);
        infinities.Remove(type);
    }

    public byte SetCategory(int type, byte category) => categories[type] = category;
    public bool TryGetCategory(int type, out byte category) => categories.TryGetValue(type, out category);

    public int SetRequirement(int type, int requirement) => requirements[type] = requirement;
    public bool TryGetRequirement(int type, out int requirement) => requirements.TryGetValue(type, out requirement);

    public long SetInfinity(int type, long infinity) => infinities[type] = infinity;
    public bool TryGetInfinity(int type, out long infinity) => infinities.TryGetValue(type, out infinity);
}

public static class InfinityManager {

    public static readonly List<Infinity> Infinities = new();
    private static readonly List<InfinityCache> s_Caches = new();

    public static int RegisterInfinity(Infinity infinity) {
        Infinities.Add(infinity);
        s_Caches.Add(new());
        return Infinities.Count - 1;
    }

    public static void ClearCache() {
        foreach (InfinityCache cache in s_Caches) cache.ClearAll();
    }
    public static void ClearCache(Item item) {
        for (int i = 0; i < s_Caches.Count; i++) s_Caches[i].ClearType(Infinities[i].Type(item));
    }

    public static byte GetCategory(this Item item, int infinityID){
        int type = Infinities[infinityID].Type(item);
        if(s_Caches[infinityID].TryGetCategory(type, out byte cat)) return cat;
        return s_Caches[infinityID].SetCategory(type, Infinities[infinityID].GetCategory(item));
    }
    public static byte GetCategory(int type, int infinityID){
        if(s_Caches[infinityID].TryGetCategory(type, out byte cat)) return cat;
        return s_Caches[infinityID].SetCategory(type, Infinities[infinityID].GetCategory(type));
    }

    public static int GetRequirement(this Item item, int infinityID){
        int type = Infinities[infinityID].Type(item);
        if(s_Caches[infinityID].TryGetRequirement(type, out int req)) return req;
        return s_Caches[infinityID].SetRequirement(type, Infinities[infinityID].GetRequirement(item));
    }
    public static int GetRequirement(int type, int infinityID){
        if(s_Caches[infinityID].TryGetRequirement(type, out int req)) return req;
        return s_Caches[infinityID].SetRequirement(type, Infinities[infinityID].GetRequirement(type));
    }

    public static long GetInfinity(this Player player, Item item, int infinityID){
        int type = Infinities[infinityID].Type(item);
        if(s_Caches[infinityID].TryGetInfinity(type, out long inf)) return inf;
        return s_Caches[infinityID].SetInfinity(type, Infinities[infinityID].GetInfinity(player, item));
    }
    public static long GetInfinity(this Player player, int type, int infinityID){
        if(s_Caches[infinityID].TryGetInfinity(type, out long inf)) return inf;
        return s_Caches[infinityID].SetInfinity(type, Infinities[infinityID].GetInfinity(player, type));
    }

    public static bool IsInfinite(long consumed, long infinity) => consumed <= infinity;
    
    public static bool HasInfinite(this Player player, Item item, long consumed, int infinityID)
        => Infinities[infinityID].Enabled && IsInfinite(consumed, player.GetInfinity(item, infinityID));
    public static bool HasInfinite(this Player player, int type, long consumed, int infinityID)
        => Infinities[infinityID].Enabled && IsInfinite(consumed, player.GetInfinity(type, infinityID));


    public delegate long AboveRequirementInfinity(long count, int requirement, params int[] args);
    public static class ARIDelegates {
        public static long NotInfinite(long count, int requirement, params int[] args) => 0;
        public static long ItemCount(long count, int requirement, params int[] args) => count;
        public static long Requirement(long count, int requirement, params int[] args) => requirement;

        public static long LargestMultiple(long count, int requirement, params int[] args)
            => count / requirement * requirement;
        public static long LargestPower(long count, int requirement, params int[] args)
            => requirement * (long)System.MathF.Pow(args[0], (int)System.MathF.Log(count / (float)requirement, args[0]));
    }

    public static long CalculateInfinity(int type, int theoricalMaxStack, long count, int requirement, float multiplier, AboveRequirementInfinity aboveRequirement = null, params int[] args)
        => CalculateInfinity(
            (int)System.MathF.Min(Globals.ConsumptionItem.MaxStack(type), theoricalMaxStack),
            count, requirement, multiplier, aboveRequirement, args
        );

    public static long CalculateInfinity(int maxStack, long count, int requirement, float multiplier, AboveRequirementInfinity aboveRequirement = null, params int[] args) {
        requirement = Utility.RequirementToItems(requirement, maxStack);
        if (requirement == 0) return -2;
        if (count < requirement) return -1;
        long infinity = count == requirement ? requirement :
            (aboveRequirement ?? ARIDelegates.Requirement).Invoke(count, requirement, args);
        return (long)(infinity * multiplier);
    }
}
