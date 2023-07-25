using System;
using System.Collections.Generic;

namespace SPIC;

public readonly record struct Requirement(long Count, float Multiplier = 1f) {
    
    public readonly bool IsNone => Count == 0 || Multiplier == 0f;

    public long Infinity(long count) => count >= Count ? (long)(count * Multiplier) : 0;

    public long CountForInfinity(long infinity) => Math.Max(Count, (int)MathF.Ceiling(infinity / Multiplier));
}

public interface IFullRequirement {
    Requirement Requirement { get; }

    string ExtraInfo();
}

// TODO localize extra
public readonly record struct FullRequirement(Requirement Requirement) : IFullRequirement {
    public string ExtraInfo() => string.Empty;
}
public readonly record struct FullRequirement<TCategory>(TCategory Category, Requirement Requirement) : IFullRequirement where TCategory : System.Enum {
    public string ExtraInfo() => Category.ToString();
}
public readonly record struct MixedRequirement(Requirement Requirement) : IFullRequirement{
    public string ExtraInfo() => "Mixed";
}
public readonly record struct CustomRequirement(Requirement Requirement) : IFullRequirement{
    public string ExtraInfo() => "Custom";
}

public readonly record struct FullInfinity(IFullRequirement FullRequirement, long Count, long Infinity) {
    public Requirement Requirement => FullRequirement.Requirement;
}

public class MetaInfinity<TMetaGroup, TConsumable> where TMetaGroup : MetaGroup<TMetaGroup, TConsumable> {
    
    public MetaInfinity() {
        _infinities = new();
        UsedGroups = new();
        Mixed = new();
    }

    public void AddGroup(ModGroup<TMetaGroup, TConsumable> group, FullInfinity infinity, bool used) {
        _infinities[group] = infinity;
        if(used) UsedGroups.Add(group);
    }
    public void AddMixed(Requirement? custom = null) {
        long count = 0;
        long reqCount = 0; float reqMult = 0f;
        foreach(ModGroup<TMetaGroup, TConsumable> group in UsedGroups){
            FullInfinity fullInfinity = _infinities[group];
            if(count == 0 || fullInfinity.Count < count) count = fullInfinity.Count;
            if(reqCount == 0 || fullInfinity.Requirement.Count > reqCount) reqCount = fullInfinity.Requirement.Count;
            if(reqMult == 0 || fullInfinity.Requirement.Multiplier < reqMult) reqMult = fullInfinity.Requirement.Multiplier;

        }
        IFullRequirement requirement;
        if(custom.HasValue){
            UsedGroups.Clear();
            requirement = new CustomRequirement(custom.Value);
        } else requirement = new MixedRequirement(new(reqCount, reqMult));
        Mixed = new(requirement, count, requirement.Requirement.Infinity(count));
    }

    public FullInfinity this[ModGroup<TMetaGroup, TConsumable> group] => _infinities[group];
    public FullInfinity Mixed { get; private set; }

    public HashSet<ModGroup<TMetaGroup, TConsumable>> UsedGroups { get; private set; }

    private readonly Dictionary<ModGroup<TMetaGroup, TConsumable>, FullInfinity> _infinities;

}

public class MetaDisplay {

    public MetaDisplay() {
        DisplayedGroups = new();
        ByMetaGroups = new();
        _infinities = new();
    }

    public (FullInfinity Infinity, long Consumed) this[IModGroup group] {
        get {
            (int type, long consumed) = _infinities[group];
            return (InfinityManager.GetFullInfinity(Terraria.Main.LocalPlayer, type, group), consumed);
        }
    }

    public void AddGroup(IModGroup group, int type, long consumed, bool exclusive) {
        if(ExclusiveContext && !exclusive) return;
        if(!ExclusiveContext && exclusive) {
            _infinities.Clear();
            ByMetaGroups.Clear();
            DisplayedGroups.Clear();
            ExclusiveContext = true;
        }

        _infinities[group] = (type, consumed);
        DisplayedGroups.Add(group);
        if(ByMetaGroups.Count == 0 || ByMetaGroups[^1][0].MetaGroup != group.MetaGroup){
            ByMetaGroups.Add(new());
        }
        ByMetaGroups[^1].Add(group);
    }

    public bool ExclusiveContext { get; private set; }

    private readonly Dictionary<IModGroup, (int, long)> _infinities;
    public List<IModGroup> DisplayedGroups { get; private set; }
    public List<List<IModGroup>> ByMetaGroups { get; private set; }
}