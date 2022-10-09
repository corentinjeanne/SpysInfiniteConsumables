using System.Collections;
using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableTypes;

namespace SPIC;

public enum FilterFlags {
    Default =   NonGlobal | Enabled,
    NonGlobal = 0b0001,
    Global =    0b0010,
    Enabled =   0b0100,
    Disabled =  0b1000
}


public static class InfinityManager {

    public static int LCM { get; private set; } = 1;

    public static void Register<TImplementation>(ConsumableType<TImplementation> type, bool global = false) where TImplementation : ConsumableType<TImplementation>, IConsumableType, new() {
        
        if (type.UID != 0) throw new System.ArgumentException("This type has already been registered", nameof(type));
        if(!global && type is not IToggleable or not IDefaultDisplay) throw new System.ArgumentException($"A non global type must implement \"{nameof(IToggleable)}\" and \"{nameof(IDefaultDisplay)}\"", nameof(type));
        int id = type.UID = global ? s_nextGlobalID-- : s_nextTypeID++;
        s_consumableTypes[id] = (IConsumableType)type;
        s_caches[id] = new();

        static int GCD(int x, int y) => x == 0 ? y : GCD(y % x, x);
        LCM = s_consumableTypes.Count * LCM / GCD(s_consumableTypes.Count, LCM);
    }

    public static IConsumableType ConsumableType(int id) => s_consumableTypes[ValidateConsumable(id)];
    public static IConsumableType ConsumableType(string mod, string Name) => s_consumableTypes.FindValue(kvp => kvp.Key > 0 && kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);


    public static bool IsTypeEnabled(IConsumableType type) => type is not IToggleable || (type.UID > 0 ? (bool)Requirements.EnabledTypes[new Configs.ConsumableTypeDefinition(type.UID)] : Requirements.EnabledGlobals[new(type.UID)]);
    public static bool IsTypeEnabled(int id) => IsTypeEnabled(ConsumableType(id));
    public static bool IsTypeUsed(this Item item, int typeID) => item.UsedConsumableTypes().Find(t => t.UID == typeID) != null;
    public static bool IsTypeUsed(int type, int typeID) => UsedConsumableTypes(type).Find(t => t.UID == typeID) != null;

    public static IEnumerable<IConsumableType> ConsumableTypes(FilterFlags flags = FilterFlags.Default, bool disableOrdering = false) => ConsumableTypes<IConsumableType>(flags, disableOrdering);
    public static IEnumerable<TConsumableType> ConsumableTypes<TConsumableType>(FilterFlags filters = FilterFlags.Default, bool disableOrdering = false) where TConsumableType: IConsumableType {
        bool enabled = filters.HasFlag(FilterFlags.Enabled);
        bool disabled = filters.HasFlag(FilterFlags.Disabled);

        bool MatchsFlags(int id, IConsumableType t) => t is TConsumableType && ((enabled && disabled) || (disabled == !IsTypeEnabled(id)));

        if (filters.HasFlag(FilterFlags.NonGlobal)) {
            if (!disableOrdering) {
                foreach (DictionaryEntry entry in Requirements.EnabledTypes) {
                    IConsumableType type = ((Configs.ConsumableTypeDefinition)entry.Key).ConsumableType;
                    if (MatchsFlags(type.UID, type)) yield return (TConsumableType)type;
                }
            } else {
                foreach ((int id, IConsumableType type) in s_consumableTypes) {
                    if (id > 0 && MatchsFlags(id, type)) yield return (TConsumableType)type;
                }
            }
        }
        if (filters.HasFlag(FilterFlags.Global)) {
            foreach ((int id, IConsumableType type) in s_consumableTypes) {
                if (id < 0 && MatchsFlags(id, type)) yield return (TConsumableType)type;
            }
        }
    }

    public static IReadOnlyList<IDefaultDisplay> UsedConsumableTypes(this Item item){
        if(s_usedTypes.TryGetValue(item.type, out IReadOnlyList<IDefaultDisplay> types)) return types;
        List<IDefaultDisplay> used = new();
        foreach (IDefaultDisplay consumableType in ConsumableTypes<IDefaultDisplay>()) {
            if(item.GetRequirement(consumableType.UID) == IConsumableType.NoRequirement) continue;
            used.Add(consumableType);
            if (Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
        }
        return s_usedTypes[item.type] = used;
    }
    public static IReadOnlyList<IDefaultDisplay> UsedConsumableTypes(int type){
        if (s_usedTypes.TryGetValue(type, out IReadOnlyList<IDefaultDisplay> types)) return types;
        List<IDefaultDisplay> used = new();
        foreach (IDefaultDisplay consumableType in ConsumableTypes<IDefaultDisplay>()) { // ordered
            if (GetRequirement(type, consumableType.UID) == IConsumableType.NoRequirement) continue;
            used.Add(consumableType);
            if(Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
        }
        return s_usedTypes[type] = used;
    }
    
    public static TCategory GetCategory<TCategory>(this Item item, int consumableID) where TCategory: System.Enum => (TCategory)item.GetCategory(consumableID);
    public static Category GetCategory(this Item item, int consumableID){
        if(s_caches[ValidateConsumable(consumableID)].categories.TryGetValue(item.type, out Category cat)) return cat;
        if(!HasCategoryOverride(item.type, consumableID, out cat)) cat = s_consumableTypes[consumableID].GetCategory(item);
        return s_caches[consumableID].categories[item.type] = cat;
    }
    public static TCategory GetCategory<TCategory>(int type, int consumableID) where TCategory : System.Enum => (TCategory)GetCategory(type, consumableID);
    public static Category GetCategory(int type, int consumableID)
        => s_caches[ValidateConsumable(consumableID)].categories.TryGetValue(type, out Category cat) ? cat : GetCategory(new Item(type), consumableID);

    public static bool HasCategoryOverride(int itemType, int consumableID, out Category category) {
        IConsumableType inf = ConsumableType(consumableID);
        category = Category.None;
        return inf is IDetectable && CategoryDetection.HasDetectedCategory(itemType, consumableID, out category);
    }

    public static int GetRequirement(this Item item, int consumableID) {
        if (s_caches[ValidateConsumable(consumableID)].requirements.TryGetValue(item.type, out int req)) return req;
        if (!HasRequirementOverride(item.type, consumableID, out req)) req = s_consumableTypes[consumableID].GetRequirement(item);
        return s_caches[consumableID].requirements[item.type] = req;
    }
    public static int GetRequirement(int type, int consumableID)
        => s_caches[ValidateConsumable(consumableID)].requirements.TryGetValue(type, out int req) ? req : GetRequirement(new Item(type), consumableID);

    public static bool HasRequirementOverride(int itemType, int consumableID, out int requirement) {
        requirement = IConsumableType.NoRequirement;
        return false;
    }

    public static long GetInfinity(this Player player, Item item, int consumableID){
        ValidateConsumable(consumableID);
        bool useCache = UseCache(player);
        if(useCache && s_caches[consumableID].localPlayerInfinities.TryGetValue(item.type, out long inf)) return inf;
        inf = s_consumableTypes[consumableID].GetInfinity(player, item);
        return useCache ? s_caches[consumableID].localPlayerInfinities[item.type] = inf : inf;
    }
    public static long GetInfinity(this Player player, int type, int consumableID)
        => ValidateConsumable(consumableID) == consumableID && UseCache(player) && s_caches[consumableID].localPlayerInfinities.TryGetValue(type, out long inf) ? inf : player.GetInfinity(new Item(type), consumableID);

    public static bool HasInfinite(this Player player, Item item, long consumablesConsumed, int id) {
        return IsTypeEnabled(id) && (id < 0 || item.IsTypeUsed(id) ?
            consumablesConsumed <= player.GetInfinity(item, id) : player.HasInfinite(item, consumablesConsumed, Mixed.ID));
    }
    public static bool HasInfinite(this Player player, int type, long consumablesConsumed, int id) {
        return IsTypeEnabled(id) && (id < 0 || IsTypeUsed(type, id) ?
            consumablesConsumed <= player.GetInfinity(type, id) : player.HasInfinite(type, consumablesConsumed, Mixed.ID));
    }

    private static int ValidateConsumable(int id) => s_consumableTypes.ContainsKey(id) ? id :
        throw new System.ArgumentOutOfRangeException(nameof(id), id, "No ConsumableType with this id exists");

    private static int s_nextTypeID = 1;
    private static int s_nextGlobalID = -1;
    private static readonly Dictionary<int, IConsumableType> s_consumableTypes = new();

    private static bool UseCache(Player player) => player == Main.LocalPlayer;
    public static void ClearCache() {
        foreach ((int _, ConsumableCache cache) in s_caches) cache.ClearAll();
        s_usedTypes.Clear();
        s_usedAmmoTypes.Clear();
    }
    public static void ClearCache(int type) {
        foreach ((int _, ConsumableCache cache) in s_caches) cache.ClearType(type);
        s_usedTypes.Remove(type);
        s_usedAmmoTypes.Clear();
    }

    private static readonly Dictionary<int, ConsumableCache> s_caches = new();
    private static readonly Dictionary<int, IReadOnlyList<IDefaultDisplay>> s_usedTypes = new();
    private static readonly Dictionary<int, IReadOnlyList<(IAmmunition, Item ammo)>> s_usedAmmoTypes = new();

    private static Configs.RequirementSettings Requirements => Configs.RequirementSettings.Instance;
    private static Configs.CategoryDetection CategoryDetection => Configs.CategoryDetection.Instance;

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

    public static long CalculateInfinity(int maxStack, long count, int requirement, float multiplier, AboveRequirementInfinity aboveRequirement = null, params int[] args) {
        requirement = Utility.RequirementToItems(requirement, maxStack);
        if (requirement == 0) return IConsumableType.NoInfinity;
        if (count < requirement) return IConsumableType.NotInfinite;
        long infinity = count == requirement ? requirement :
            (aboveRequirement ?? ARIDelegates.ItemCount).Invoke(count, requirement, args);
        return (long)(infinity * multiplier);
    }

    public static Configs.ConsumableTypeDefinition ToDefinition(this IConsumableType type) => new(type.Mod, type.Name);

}

internal sealed class ConsumableCache {
    public readonly Dictionary<int, Category> categories = new();
    public readonly Dictionary<int, int> requirements = new();
    public readonly Dictionary<int, long> localPlayerInfinities = new();
    public readonly Dictionary<int, long> localPlayerFullInfinities = new();

    public void ClearAll() {
        categories.Clear();
        requirements.Clear();
        localPlayerInfinities.Clear();
        localPlayerFullInfinities.Clear();
    }
    public void ClearType(int type) {
        categories.Remove(type);
        requirements.Remove(type);
        localPlayerInfinities.Remove(type);
        localPlayerFullInfinities.Remove(type);
    }
}