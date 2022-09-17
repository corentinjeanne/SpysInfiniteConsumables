using System.Collections.Generic;
using Terraria;
using SPIC.ConsumableTypes;
using SPIC.Infinities;
using SPIC.Configs;
using Terraria.ModLoader;

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

public static class InfinityManager {

    public static void RegisterInfinity(Infinity infinity) {
        if(infinity.UID != 0) throw new System.ArgumentException("This infinity has already been registered", nameof(infinity));
        int id = infinity.UID = s_infinities.Count + 1;
        s_infinities[id] = infinity;
        Requirements.Infinities.TryAdd(infinity.ToDefinition(), infinity.DefaultValue);
    }

    public static Infinity Infinity(int id) => s_infinities[ValidateInfinityID(id)];
    public static Infinity Infinity(string mod, string Name) => s_infinities.FindValue(kvp => kvp.Key > 0 && kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);
    public static int ValidateInfinityID(int id) => id > 0 && s_infinities.ContainsKey(id) ? id :
        throw new System.ArgumentOutOfRangeException(nameof(id), id, "No infinity with this id exists");

    public static IEnumerable<Infinity> Infinities {
        get {
            foreach ((int id, Infinity inf) in s_infinities) {
                if (id > 0) yield return inf;
            }
        }
    }

    public static bool IsInfinityEnabled(int id) => Requirements.Infinities[Infinity(id).ToDefinition()];
    public static IReadOnlyList<int> PairedConsumableType(int infinityID) => s_pairedInfinity.Reverse(ValidateInfinityID(infinityID));

    private static readonly Dictionary<int, Infinity> s_infinities = new();
    private static readonly Dictionary<int, int> s_pairedInfinity = new();

    public static void RegisterConsumableType(ConsumableType type, int infinityID) {
        if (type.UID != 0) throw new System.ArgumentException("This consumable has already been registered", nameof(type));
        int id = type.UID = _typesCount++;
        s_types[id] = type;
        s_caches[id] = new();
        s_pairedInfinity[id] = ValidateInfinityID(infinityID);

        ConsumableTypeDefinition key = type.ToDefinition();
        if (!Requirements.Requirements.Contains(key)) {
            Requirements.Requirements.Add(key, type.CreateRequirements());
            Requirements.SaveConfig();
        }

        if (type is IDetectable) {
            CategoryDetection.DetectedCategories.TryAdd(type.ToDefinition(), new());
            CategoryDetection.SaveConfig();
        }
    }

    // TODO change to global type

    // Globals : allways on, but never used
    public static void RegisterGlobalConsumableType(ConsumableType type){
        int id = type.UID = s_globalsCount--;
        s_types[id] = type;
        s_caches[id] = new();
    }

    private static readonly Dictionary<int, ConsumableType> s_types = new();

    public static ConsumableType ConsumableType(int id) => s_types[ValidateConsumable(id)];
    public static ConsumableType ConsumableType(string mod, string Name) => s_types.FindValue(kvp => kvp.Key > 0 && kvp.Value.Mod.Name == mod && kvp.Value.Name == Name);
    public static int ValidateConsumable(int id) => ValidateConsumable(id, false);
    internal static int ValidateConsumable(int id, bool includeHidden) => (includeHidden || id >= 0) && s_types.ContainsKey(id) ? id :
        throw new System.ArgumentOutOfRangeException(nameof(id), id, "No ConsumableType with this id exists");

    public static int PairedInfinity(int consumableID) => s_pairedInfinity[ValidateConsumable(consumableID)];

    public static bool IsTypeGlobal(int consumableID) => ValidateConsumable(consumableID, true) < 0;
    public static bool IsTypeEnabled(int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID));
    public static bool IsTypeEnabled(this Item item, int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID)) && item.GetRequirement(consumableID) != SPIC.ConsumableTypes.ConsumableType.NoRequirement;
    public static bool IsTypeEnabled(int type, int consumableID) => IsInfinityEnabled(PairedInfinity(consumableID)) && GetRequirement(type, consumableID) != SPIC.ConsumableTypes.ConsumableType.NoRequirement;

    public static IEnumerable<ConsumableType> ConsumableTypes {
        get {
            foreach ((int id, ConsumableType type) in s_types) {
                if (id > 0) yield return type;
            }
        }
    }

    public static IEnumerable<int> EnabledTypes() {
        foreach (ConsumableType type in ConsumableTypes) {
            if (type is not null && IsTypeEnabled(type.UID)) yield return type.UID;
        }
    }

    private static int _typesCount = 1;
    private static int s_globalsCount = -1;

    private static bool UseCache(Player player) => player == Main.LocalPlayer;

    private static readonly Dictionary<int, ConsumableCache> s_caches = new();
    private static readonly Dictionary<int, int[]> s_usedTypes = new();
    private static readonly Dictionary<int, int[]> s_usedAmmoTypes = new();

    public static void ClearCache() {
        foreach ((int _, ConsumableCache cache) in s_caches) cache.ClearAll();
        s_usedTypes.Clear();
        s_usedAmmoTypes.Clear();
    }
    public static void ClearCache(int type) {
        foreach((int _, ConsumableCache cache) in s_caches) cache.ClearType(type);
        s_usedTypes.Remove(type);
        s_usedAmmoTypes.Clear();
    }

    // ? Use IEnumerable
    public static IReadOnlyList<int> UsedConsumableTypes(this Item item){
        if(s_usedTypes.TryGetValue(item.type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
            if(item.GetRequirement(typeID) != SPIC.ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_usedTypes[item.type] = used.ToArray();
    }
    public static IReadOnlyList<int> UsedConsumableTypes(int type){
        if (s_usedTypes.TryGetValue(type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(Requirements.MaxConsumableTypes != 0 && used.Count >= Requirements.MaxConsumableTypes) break;
            if(GetRequirement(type, typeID) != SPIC.ConsumableTypes.ConsumableType.NoRequirement) used.Add(typeID);
        }
        return s_usedTypes[type] = used.ToArray();
    }
    public static IReadOnlyList<int> UsedAmmoTypes(this Item item){
        if(s_usedAmmoTypes.TryGetValue(item.type, out int[] types)) return types;
        List<int> used = new();
        foreach (int typeID in EnabledTypes()) {
            if(ConsumableType(typeID) is IAmmunition aType && aType.ConsumesAmmo(item)) used.Add(typeID);
        }
        return s_usedAmmoTypes[item.type] = used.ToArray();
    }

    public static bool IsTypeUsed(this Item item, int typeID) => item.UsedConsumableTypes().IndexOf(typeID) != -1;
    public static bool IsTypeUsed(int type, int typeID) => UsedConsumableTypes(type).IndexOf(typeID) != -1;

    public static byte GetCategory(this Item item, int consumableID){
        if(s_caches[ValidateConsumable(consumableID, true)].categories.TryGetValue(item.type, out byte cat)) return cat;
        if(!HasCategoryOverride(item.type, consumableID, out cat)) cat = s_types[consumableID].GetCategory(item);
        return s_caches[consumableID].categories[item.type] = cat;
    }
    public static byte GetCategory(int type, int consumableID)
        => s_caches[ValidateConsumable(consumableID, true)].categories.TryGetValue(type, out byte cat) ? cat :
            GetCategory(new Item(type), consumableID);

    public static bool HasCategoryOverride(int itemType, int consumableID, out byte category) {
        ConsumableType inf = ConsumableType(consumableID);
        category = SPIC.ConsumableTypes.ConsumableType.NoCategory;
        return (inf is ICustomizable && Requirements.HasCustomCategory(itemType, consumableID, out category)) || (inf is IDetectable && CategoryDetection.HasDetectedCategory(itemType, consumableID, out category));
    }

    public static int GetRequirement(this Item item, int consumableID) {
        if (s_caches[ValidateConsumable(consumableID, true)].requirements.TryGetValue(item.type, out int req)) return req;
        if (!HasRequirementOverride(item.type, consumableID, out req)) req = s_types[consumableID].GetRequirement(item);
        return s_caches[consumableID].requirements[item.type] = req;
    }
    public static int GetRequirement(int type, int consumableID)
        => s_caches[ValidateConsumable(consumableID, true)].requirements.TryGetValue(type, out int req) ? req : GetRequirement(new Item(type), consumableID);

    public static bool HasRequirementOverride(int itemType, int consumableID, out int requirement) {
        requirement = SPIC.ConsumableTypes.ConsumableType.NoRequirement;
        return ConsumableType(consumableID) is ICustomizable && Requirements.HasCustomRequirement(itemType, consumableID, out requirement);
    }

    public static long GetInfinity(this Player player, Item item, int consumableID){
        ValidateConsumable(consumableID, true);
        bool useCache = UseCache(player);
        if(useCache && s_caches[consumableID].localPlayerInfinities.TryGetValue(item.type, out long inf)) return inf;
        inf = s_types[consumableID].GetInfinity(player, item);
        return useCache ? s_caches[consumableID].localPlayerInfinities[item.type] = inf : inf;
    }
    public static long GetInfinity(this Player player, int type, int consumableID)
        => ValidateConsumable(consumableID, true) == consumableID && UseCache(player) && s_caches[consumableID].localPlayerInfinities.TryGetValue(type, out long inf) ? inf : player.GetInfinity(new Item(type), consumableID);
    

    public static long GetFullInfinity(this Player player, Item item, int consumableID){
        if(s_types[ValidateConsumable(consumableID, true)] is not IPartialInfinity pType) return 0;
        bool useCache = UseCache(player);
        if (useCache && s_caches[consumableID].localPlayerFullInfinities.TryGetValue(item.type, out long inf)) return inf;
        inf = pType.GetFullInfinity(player, item);
        return useCache ? s_caches[consumableID].localPlayerFullInfinities[item.type] = inf : inf;
    }
    public static long GetFullInfinity(this Player player, int type, int consumableID)
        => s_types[ValidateConsumable(consumableID, true)] is not IPartialInfinity ? 0 : (UseCache(player) && s_caches[consumableID].localPlayerFullInfinities.TryGetValue(type, out long inf) ? inf : player.GetFullInfinity(new Item(type), consumableID));

    public static bool IsInfinite(long consumed, long infinity) => (consumed < 0 ? 0 : consumed) <= infinity;

    public static bool HasInfinite(this Player player, Item item, long consumablesConsumed, int typeID) {
        if(IsTypeGlobal(typeID) || item.IsTypeUsed(typeID)) return IsInfinite(consumablesConsumed, player.GetInfinity(item, typeID));
        return item.IsTypeEnabled(typeID) && player.HasInfinite(item, consumablesConsumed, Mixed.ID);
    }
    public static bool HasInfinite(this Player player, int type, long consumablesConsumed, int typeID) {
        if (IsTypeGlobal(typeID) || IsTypeUsed(type, typeID)) return IsInfinite(consumablesConsumed, player.GetInfinity(type, typeID));
        return IsTypeEnabled(type, typeID) && player.HasInfinite(type, consumablesConsumed, Mixed.ID);
    }
    public static bool HasFullyInfinite(this Player player, Item item, int typeID) {
        if (IsTypeGlobal(typeID) || item.IsTypeUsed(typeID)) return IsInfinite(player.GetFullInfinity(item, typeID), player.GetInfinity(item, typeID));
        return item.IsTypeEnabled(typeID) && player.HasFullyInfinite(item, Mixed.ID);
    }
    public static bool HasFullyInfinite(this Player player, int type, int typeID){
        if (IsTypeGlobal(typeID) || IsTypeUsed(type, typeID)) return IsInfinite(player.GetFullInfinity(type, typeID), player.GetInfinity(type, typeID));
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
        if (requirement == 0) return SPIC.ConsumableTypes.ConsumableType.NoInfinity;
        if (count < requirement) return SPIC.ConsumableTypes.ConsumableType.NotInfinite;
        long infinity = count == requirement ? requirement :
            (aboveRequirement ?? ARIDelegates.ItemCount).Invoke(count, requirement, args);
        return (long)(infinity * multiplier);
    }

    private static Configs.RequirementSettings Requirements => Configs.RequirementSettings.Instance;
    private static Configs.CategoryDetection CategoryDetection => Configs.CategoryDetection.Instance;
}