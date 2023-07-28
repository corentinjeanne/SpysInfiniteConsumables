using System.Collections.Generic;

namespace SPIC;


public sealed class GroupInfinity {
    
    public GroupInfinity() {
        _infinities = new();
        UsedInfinities = new();
        Mixed = FullInfinity.None;
    }

    public void Add(IInfinity infinity, FullInfinity fullInfinity, bool used) {
        _infinities[infinity] = fullInfinity;
        if(used) UsedInfinities.Add(infinity);
    }
    public void AddMixed(Requirement? custom = null) {
        long count = 0;
        long reqCount = 0; float reqMult = 0f;
        foreach(IInfinity infinity in UsedInfinities){
            FullInfinity fullInfinity = _infinities[infinity];
            if(count == 0 || fullInfinity.Count < count) count = fullInfinity.Count;
            if(reqCount == 0 || fullInfinity.Requirement.Count > reqCount) reqCount = fullInfinity.Requirement.Count;
            if(reqMult == 0 || fullInfinity.Requirement.Multiplier < reqMult) reqMult = fullInfinity.Requirement.Multiplier;

        }
        InfinityOverride extra;
        Requirement requirement;
        if(custom.HasValue){
            extra = new("Custom");
            requirement = custom.Value;
            UsedInfinities.Clear();
        } else {
            extra = new("Mixed");
            requirement = new(reqCount, reqMult);
        }
        Mixed = FullInfinity.With(requirement, count, requirement.Infinity(count), extra);
    }

    public FullInfinity this[IInfinity infinity] => _infinities[infinity];
    public FullInfinity Mixed { get; private set; }

    public HashSet<IInfinity> UsedInfinities { get; private set; }

    private readonly Dictionary<IInfinity, FullInfinity> _infinities;

}

public sealed class ItemDisplay {

    public ItemDisplay() {
        DisplayedInfinities = new();
        InfinitiesByGroup = new();
        _infinities = new();
    }

    public (FullInfinity Infinity, long Consumed) this[IInfinity infinity] {
        get {
            (int type, long consumed) = _infinities[infinity];
            return (InfinityManager.GetFullInfinity(Terraria.Main.LocalPlayer, type, infinity), consumed);
        }
    }

    public void Add(IInfinity infinity, int type, long consumed, bool exclusive) { // ? compte the infinity already
        if(ExclusiveContext && !exclusive) return;
        if(!ExclusiveContext && exclusive) {
            _infinities.Clear();
            InfinitiesByGroup.Clear();
            DisplayedInfinities.Clear();
            ExclusiveContext = true;
        }

        _infinities[infinity] = (type, consumed);
        DisplayedInfinities.Add(infinity);
        if(InfinitiesByGroup.Count == 0 || InfinitiesByGroup[^1][0].Group != infinity.Group){
            InfinitiesByGroup.Add(new());
        }
        InfinitiesByGroup[^1].Add(infinity);
    }

    public bool ExclusiveContext { get; private set; }

    private readonly Dictionary<IInfinity, (int, long)> _infinities;
    public List<IInfinity> DisplayedInfinities { get; private set; }
    public List<List<IInfinity>> InfinitiesByGroup { get; private set; }
}