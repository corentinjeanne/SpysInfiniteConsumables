using System.Collections.Generic;
using Terraria;

using SPIC.ConsumableTypes;
using SPIC.Infinities;

namespace SPIC;

internal sealed class ConsumableCache {
    public readonly Dictionary<int, byte> categories = new();
    public readonly Dictionary<int, int> requirements = new();
    public readonly Dictionary<int, long> localPlayerInfinities = new();

    public void ClearAll(){
        categories.Clear();
        requirements.Clear();
        localPlayerInfinities.Clear();
    }
    public void ClearType(int type){
        categories.Remove(type);
        requirements.Remove(type);
        localPlayerInfinities.Remove(type);
    }

    public byte SetCategory(int type, byte category) => categories[type] = category;
    public bool TryGetCategory(int type, out byte category) => categories.TryGetValue(type, out category);

    public int SetRequirement(int type, int requirement) => requirements[type] = requirement;
    public bool TryGetRequirement(int type, out int requirement) => requirements.TryGetValue(type, out requirement);

    public long SetInfinity(int type, long infinity) => localPlayerInfinities[type] = infinity;
    public bool TryGetInfinity(int type, out long infinity) => localPlayerInfinities.TryGetValue(type, out infinity);
}


// ? use ConsumableType instead of int for better performances
public static class InfinityManager {


    public static int RegisterInfinity(Infinity infinity) {
        Configs.Requirements.Instance.Infinities.TryAdd(infinity.Name, infinity.DefaultValue);
        Configs.Requirements.Instance.Reqs.TryAdd(infinity.Name, new());
        s_Infinities.Add(infinity);
        s_PairedTypes.Add(new());
        return s_Infinities.Count-1;
    }
    public static Infinity Infinity(int id) => id < s_Infinities.Count ? s_Infinities[id] : null;
    public static Infinity Infinity(string Name) => s_Infinities.Find(i => i.Name == Name);
    public static List<int> PairedConsumableType(int infinityID) => s_PairedTypes[infinityID];
    public static bool IsInfinityEnabled(int infinityID) => Configs.Requirements.Instance.Infinities[Infinity(infinityID).Name];
    public static int InfinityCount => s_Infinities.Count;

    private static readonly List<Infinity> s_Infinities = new();
    private static readonly List<List<int>> s_PairedTypes = new();


    public static int RegisterConsumableType(ConsumableType type, int infinityID) {
        if(!Configs.Requirements.Instance.ConsumableTypePriority.Contains(type.Name))
            Configs.Requirements.Instance.ConsumableTypePriority.Add(type.Name);
            
        s_Types.Add(type);
        s_Caches.Add(new());
        int id = s_Types.Count - 1;
        s_PairedTypes[infinityID].Add(id);
        Configs.Requirements.Instance.Reqs[Infinity(infinityID).Name].TryAdd(type.Name, type.CreateRequirements());
        if(type.CategoryDetection) Configs.CategoryDetection.Instance.DetectedCategories.TryAdd(type.Name, new());
        return id;
    }
    public static ConsumableType ConsumableType(int id) => id < s_Types.Count ? s_Types[id] : null;
    public static ConsumableType ConsumableType(string Name) => s_Types.Find(i => i.Name == Name);
    public static int[] EnabledTypes(){
        List<int> enabled = new();
        foreach(string name in Configs.Requirements.Instance.ConsumableTypePriority){
            ConsumableType type = ConsumableType(name);
            if(type is not null && IsTypeEnabled(type.UID)) enabled.Add(type.UID);
        }
        return enabled.ToArray();
    }
    public static bool IsTypeEnabled(int consumableID) => IsInfinityEnabled(s_PairedTypes.FindIndex(i => i.Contains(consumableID)));
    public static int ConsumableTypesCount => s_Types.Count;

    private static readonly List<ConsumableType> s_Types = new();
    private static readonly List<ConsumableCache> s_Caches = new();

    public static void ClearCache() {
        foreach (ConsumableCache cache in s_Caches) cache.ClearAll();
        s_UsedTypes.Clear();
    }
    public static void ClearCache(int type) {
        for (int i = 0; i < s_Caches.Count; i++) s_Caches[i].ClearType(type);
        s_UsedTypes.Remove(type);
    }

    private static readonly Dictionary<int, int[]> s_UsedTypes = new();

    public static int[] UsedConsumableTypes(this Item item){
        if(s_UsedTypes.TryGetValue(item.type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Configs.Requirements.Instance.MaxInfinities != 0 && used.Count >= Configs.Requirements.Instance.MaxInfinities) break;
            if(item.GetRequirement(typeID) != ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_UsedTypes[item.type] = used.ToArray();
    }
    public static int[] UsedConsumableTypes(int type){
        if (s_UsedTypes.TryGetValue(type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Configs.Requirements.Instance.MaxInfinities != 0 && used.Count >= Configs.Requirements.Instance.MaxInfinities) break;
            if(GetRequirement(type, typeID) != ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_UsedTypes[type] = used.ToArray();
    }

    public static byte GetCategory(this Item item, int consumableID){
        if(s_Caches[consumableID].TryGetCategory(item.type, out byte cat)) return cat;
        if(!HasCategoryOverride(item.type, consumableID, out cat)) cat = s_Types[consumableID].GetCategory(item);
        return s_Caches[consumableID].SetCategory(item.type, cat);
    }
    public static byte GetCategory(int type, int consumableID)
        => s_Caches[consumableID].TryGetCategory(type, out byte cat) ? cat :
            GetCategory(new Item(type), consumableID);

    public static bool HasCategoryOverride(int itemType, int infinityID, out byte category)
        => (category = CategoryOverride(itemType, infinityID)) != ConsumableTypes.ConsumableType.UnknownCategory;
    
    public static byte CategoryOverride(int itemType, int infinityID) { // TODO >>> customs
        ConsumableType inf = s_Types[infinityID];
        if((inf.Customs && false) || (inf.CategoryDetection && Configs.CategoryDetection.Instance.HasDetectedCategory(itemType, infinityID, out byte cat)))
            return cat;
        return ConsumableTypes.ConsumableType.UnknownCategory;
    }

    public static int GetRequirement(this Item item, int infinityID) {
        if (s_Caches[infinityID].TryGetRequirement(item.type, out int req)) return req;
        return s_Caches[infinityID].SetRequirement(item.type, s_Types[infinityID].GetRequirement(item));
    }

    public static int GetRequirement(int type, int consumableID)
        => s_Caches[consumableID].TryGetRequirement(type, out int req) ? req :
            GetRequirement(new Item(type), consumableID);

    public static long GetInfinity(this Player player, Item item, int consumableID){
        if(s_Caches[consumableID].TryGetInfinity(item.type, out long inf)) return inf;
        return s_Caches[consumableID].SetInfinity(item.type, s_Types[consumableID].GetInfinity(player, item));
    }
    public static long GetInfinity(this Player player, int type, int consumableID)
        => s_Caches[consumableID].TryGetInfinity(type, out long inf) ? inf :
            player.GetInfinity(new Item(type), consumableID);

    public static bool IsInfinite(long consumed, long infinity) => consumed <= infinity;

    // ? add cache
    public static bool HasInfinite(this Player player, Item item, long consumablesConsumed, int typeID)
        => IsTypeEnabled(typeID) && (
            System.Array.IndexOf(item.UsedConsumableTypes(), typeID) != -1 ? IsInfinite(consumablesConsumed, player.GetInfinity(item, typeID)) :
                player.HasFullyInfinite(item, consumablesConsumed));

    public static bool HasInfinite(this Player player, int type, long consumablesConsumed, int typeID)
        => IsTypeEnabled(typeID) && (
            System.Array.IndexOf(UsedConsumableTypes(type), typeID) != -1 ? IsInfinite(consumablesConsumed, player.GetInfinity(type, typeID)) :
                player.HasFullyInfinite(type, consumablesConsumed));

    // ? add cache
    public static bool HasFullyInfinite(this Player player, Item item, long itemsConsumed){
        foreach(int usedID in item.UsedConsumableTypes()){
            if(!player.HasInfinite(item, itemsConsumed, usedID)) return false;
        }
        return true;
    }
    public static bool HasFullyInfinite(this Player player, int type, long itemsConsumed){
        foreach(int usedID in UsedConsumableTypes(type)){
            if(!player.HasInfinite(type, itemsConsumed, usedID)) return false;
        }
        return true;
    }

    public delegate long AboveRequirementInfinity(long count, int requirement, params int[] args);
    public static class ARIDelegates {
        public static long NotInfinite(long _, int _1, params int[] _2) => 0;
        public static long ItemCount(long count, int _, params int[] _1) => count;
        public static long Requirement(long _, int requirement, params int[] _1) => requirement;

        public static long LargestMultiple(long count, int requirement, params int[] _)
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
        requirement = Utility.RequirementToItems(requirement, maxStack); // req = -10, 100 => 1000
        if (requirement == 0) return ConsumableTypes.ConsumableType.NoInfinity;
        if (count < requirement) return ConsumableTypes.ConsumableType.NotInfinite;
        long infinity = count == requirement ? requirement :
            (aboveRequirement ?? ARIDelegates.Requirement).Invoke(count, requirement, args);
        return (long)(infinity * multiplier);
    }
}