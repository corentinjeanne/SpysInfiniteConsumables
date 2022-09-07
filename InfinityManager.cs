using System.Collections.Generic;
using Terraria;

using SPIC.ConsumableTypes;
using SPIC.Infinities;

namespace SPIC;

internal sealed class ConsumableCache {
    public readonly Dictionary<int, byte> categories = new();
    public readonly Dictionary<int, int> requirements = new();
    public readonly Dictionary<int, long> localPlayerInfinities = new();
    public readonly Dictionary<int, long> localPlayerFullInfinities = new();

    public void ClearAll(){
        categories.Clear();
        requirements.Clear();
        localPlayerInfinities.Clear();
        localPlayerFullInfinities.Clear();
    }
    public void ClearType(int type){
        categories.Remove(type);
        requirements.Remove(type);
        localPlayerFullInfinities.Remove(type);
    }
}


// ? use ConsumableType instead of int for better performances
public static class InfinityManager {


    public static void RegisterInfinity(Infinity infinity) {
        infinity.UID = s_Infinities.Count;
        s_Infinities.Add(infinity);
        s_PairedTypes.Add(new());

        Requirements.Infinities.TryAdd(infinity.Name, infinity.DefaultValue);
        Requirements.Reqs.TryAdd(infinity.Name, new());
    }


    public static Infinity Infinity(int id) => s_Infinities[ValidateInfinity(id)];
    public static Infinity Infinity(string Name) => s_Infinities.Find(i => i.Name == Name);
    public static int ValidateInfinity(int id) => id >= 0 && id < InfinityCount ? id :
        throw new System.ArgumentOutOfRangeException(nameof(id), id, "No infinity with this id exists");

    public static int InfinityCount => s_Infinities.Count;
    public static bool IsInfinityEnabled(int id) => Requirements.Infinities[Infinity(id).Name];
    public static List<int> PairedConsumableType(int infinityID) => s_PairedTypes[ValidateInfinity(infinityID)];

    private static readonly List<Infinity> s_Infinities = new();
    private static readonly List<List<int>> s_PairedTypes = new();


    public static void RegisterConsumableType(ConsumableType type, int infinityID) {
        int id = type.UID = ConsumableTypesCount++;
        s_Types[id] = type;
        s_Caches[id] = new();
        s_PairedTypes[infinityID].Add(id);

        if(!Requirements.ConsumableTypePriority.Contains(type.Name))
            Requirements.ConsumableTypePriority.Add(type.Name);
        Requirements.Reqs[s_Infinities[infinityID].Name].TryAdd(type.Name, type.CreateRequirements());
        Requirements.SaveConfig();
       
        if (type is IDetectable) {
            CategoryDetection.DetectedCategories.TryAdd(type.Name, new());
            CategoryDetection.SaveConfig();
        }
    }
    public static void RegisterHiddenConsumableType(ConsumableType type){
        int id = type.UID = s_HiddenCount--;
        s_Types[id] = type;
        s_Caches[id] = new();
    }

    private static readonly Dictionary<int, ConsumableType> s_Types = new();

    public static ConsumableType ConsumableType(int id) => s_Types[ValidateConsumable(id)];
    public static ConsumableType ConsumableType(string Name) => s_Types.FindValue(kvp => kvp.Key >= 0 && kvp.Value.Name == Name);
    public static int ValidateConsumable(int id) => ValidateConsumable(id, false);
    internal static int ValidateConsumable(int id, bool includeHidden) => (includeHidden || id >= 0) && s_Types.ContainsKey(id) ? id :
        throw new System.ArgumentOutOfRangeException(nameof(id), id, "No ConsumableType with this id exists");

    public static int PairedInfinity(int consumableID){
        ValidateConsumable(consumableID);
        return s_PairedTypes.FindIndex(i => i.Contains(consumableID));
    }  

    public static int[] EnabledTypes(){
        List<int> enabled = new();
        foreach(string name in Requirements.ConsumableTypePriority){
            ConsumableType type = ConsumableType(name);
            if(type is not null && IsTypeEnabled(type.UID)) enabled.Add(type.UID);
        }
        return enabled.ToArray();
    }

    public static bool IsTypeEnabled(int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID));
    public static bool IsTypeHidden(int consumableID) => ValidateConsumable(consumableID, true) < 0;
    public static bool IsTypeEnabled(this Item item, int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID)) && item.GetRequirement(consumableID) != ConsumableTypes.ConsumableType.NoRequirement;
    public static bool IsTypeEnabled(int type, int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID)) && GetRequirement(type, consumableID) != ConsumableTypes.ConsumableType.NoRequirement;

    public static int ConsumableTypesCount { get; private set; } = 0;
    private static int s_HiddenCount = -2;

    private static bool UseCache(Player player) => player == Main.LocalPlayer;

    private static readonly Dictionary<int, ConsumableCache> s_Caches = new();
    private static readonly Dictionary<int, int[]> s_UsedTypes = new();
    private static readonly Dictionary<int, int[]> s_UsedAmmoTypes = new();

    public static void ClearCache() {
        foreach ((int _, ConsumableCache cache) in s_Caches) cache.ClearAll();
        s_UsedTypes.Clear();
        s_UsedAmmoTypes.Clear();
    }
    public static void ClearCache(int type) {
        foreach((int _, ConsumableCache cache) in s_Caches) cache.ClearType(type);
        s_UsedTypes.Remove(type);
        s_UsedAmmoTypes.Clear();
    }

    public static int[] UsedConsumableTypes(this Item item){
        if(s_UsedTypes.TryGetValue(item.type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Requirements.MaxInfinities != 0 && used.Count >= Requirements.MaxInfinities) break;
            if(item.GetRequirement(typeID) != ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_UsedTypes[item.type] = used.ToArray();
    }
    public static int[] UsedConsumableTypes(int type){
        if (s_UsedTypes.TryGetValue(type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Requirements.MaxInfinities != 0 && used.Count >= Requirements.MaxInfinities) break;
            if(GetRequirement(type, typeID) != ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_UsedTypes[type] = used.ToArray();
    }
    public static int[] UsedAmmoTypes(this Item item){
        if(s_UsedAmmoTypes.TryGetValue(item.type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(ConsumableType(typeID) is IAmmunition aType && aType.ConsumesAmmo(item)) used.Add(typeID);
        }
        return s_UsedAmmoTypes[item.type] = used.ToArray();
    }

    public static bool IsTypeUsed(this Item item, int typeID) => System.Array.IndexOf(item.UsedConsumableTypes(), typeID) != -1;
    public static bool IsTypeUsed(int type, int typeID) => System.Array.IndexOf(UsedConsumableTypes(type), typeID) != -1;

    public static byte GetCategory(this Item item, int consumableID){
        if(s_Caches[ValidateConsumable(consumableID, true)].categories.TryGetValue(item.type, out byte cat)) return cat;
        if(!HasCategoryOverride(item.type, consumableID, out cat)) cat = s_Types[consumableID].GetCategory(item);
        return s_Caches[consumableID].categories[item.type] = cat;
    }
    public static byte GetCategory(int type, int consumableID)
        => s_Caches[ValidateConsumable(consumableID, true)].categories.TryGetValue(type, out byte cat) ? cat :
            GetCategory(new Item(type), consumableID);

    public static bool HasCategoryOverride(int itemType, int consumableID, out byte category)
        => (category = CategoryOverride(itemType, consumableID)) != ConsumableTypes.ConsumableType.UnknownCategory;
    
    public static byte CategoryOverride(int itemType, int consumableID) { // TODO customs
        ConsumableType inf = s_Types[ValidateConsumable(consumableID, true)];
        if((inf is ICustomizable c && false) || (inf is IDetectable && CategoryDetection.HasDetectedCategory(itemType, consumableID, out byte cat)))
            return cat;
        return ConsumableTypes.ConsumableType.UnknownCategory;
    }

    public static int GetRequirement(this Item item, int consumableID) {
        if (s_Caches[ValidateConsumable(consumableID, true)].requirements.TryGetValue(item.type, out int req)) return req;
        return s_Caches[consumableID].requirements[item.type] = s_Types[consumableID].GetRequirement(item);
    }
    public static int GetRequirement(int type, int consumableID)
        => s_Caches[ValidateConsumable(consumableID, true)].requirements.TryGetValue(type, out int req) ? req : GetRequirement(new Item(type), consumableID);

    public static long GetInfinity(this Player player, Item item, int consumableID){
        ValidateConsumable(consumableID, true);
        bool useCache = UseCache(player);
        if(useCache && s_Caches[consumableID].localPlayerInfinities.TryGetValue(item.type, out long inf)) return inf;
        inf = s_Types[consumableID].GetInfinity(player, item);
        return useCache ? s_Caches[consumableID].localPlayerInfinities[item.type] = inf : inf;
    }
    public static long GetInfinity(this Player player, int type, int consumableID)
        => ValidateConsumable(consumableID, true) == consumableID && UseCache(player) && s_Caches[consumableID].localPlayerInfinities.TryGetValue(type, out long inf) ? inf : player.GetInfinity(new Item(type), consumableID);
    

    public static long GetFullInfinity(this Player player, Item item, int consumableID){
        if(s_Types[ValidateConsumable(consumableID, true)] is not IPartialInfinity pType) return 0;
        bool useCache = UseCache(player);
        if (useCache && s_Caches[consumableID].localPlayerFullInfinities.TryGetValue(item.type, out long inf)) return inf;
        inf = pType.GetFullInfinity(player, item);
        return useCache ? s_Caches[consumableID].localPlayerFullInfinities[item.type] = inf : inf;
    }
    public static long GetFullInfinity(this Player player, int type, int consumableID)
        => s_Types[ValidateConsumable(consumableID, true)] is not IPartialInfinity ? 0 : (UseCache(player) && s_Caches[consumableID].localPlayerFullInfinities.TryGetValue(type, out long inf) ? inf : player.GetFullInfinity(new Item(type), consumableID));

    public static bool IsInfinite(long consumed, long infinity) => (consumed < 0 ? 0 : consumed) <= infinity;

    public static bool HasInfinite(this Player player, Item item, long consumablesConsumed, int typeID) {
        if(IsTypeHidden(typeID) || item.IsTypeUsed(typeID)) return IsInfinite(consumablesConsumed, player.GetInfinity(item, typeID));
        return item.IsTypeEnabled(typeID) && player.HasInfinite(item, consumablesConsumed, Mixed.ID);
    }
    public static bool HasInfinite(this Player player, int type, long consumablesConsumed, int typeID) {
        if (IsTypeHidden(typeID) || IsTypeUsed(type, typeID)) return IsInfinite(consumablesConsumed, player.GetInfinity(type, typeID));
        return IsTypeEnabled(type, typeID) && player.HasInfinite(type, consumablesConsumed, Mixed.ID);
    }
    public static bool HasFullyInfinite(this Player player, Item item, int typeID) {
        if (IsTypeHidden(typeID) || item.IsTypeUsed(typeID)) return IsInfinite(player.GetFullInfinity(item, typeID), player.GetInfinity(item, typeID));
        return item.IsTypeEnabled(typeID) && player.HasFullyInfinite(item, Mixed.ID);
    }
    public static bool HasFullyInfinite(this Player player, int type, int typeID){
        if (IsTypeHidden(typeID) || IsTypeUsed(type, typeID)) return IsInfinite(player.GetFullInfinity(type, typeID), player.GetInfinity(type, typeID));
        return IsTypeEnabled(type, typeID) && player.HasFullyInfinite(type, Mixed.ID);
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
        requirement = Utility.RequirementToItems(requirement, maxStack);
        if (requirement == 0) return ConsumableTypes.ConsumableType.NoInfinity;
        if (count < requirement) return ConsumableTypes.ConsumableType.NotInfinite;
        long infinity = count == requirement ? requirement :
            (aboveRequirement ?? ARIDelegates.ItemCount).Invoke(count, requirement, args);
        return (long)(infinity * multiplier);
    }

    private static Configs.Requirements Requirements => Configs.Requirements.Instance;
    private static Configs.CategoryDetection CategoryDetection => Configs.CategoryDetection.Instance;
}