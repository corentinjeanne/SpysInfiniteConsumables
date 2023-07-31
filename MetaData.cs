using System.Collections.Generic;

namespace SPIC;

// TODO option to reduce cache to minimum

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
        string extra;
        Requirement requirement;
        if(custom.HasValue){
            extra = "Custom";
            requirement = custom.Value;
            UsedInfinities.Clear();
        } else {
            extra = "Mixed";
            requirement = new(reqCount, reqMult);
        }
        Mixed = FullInfinity.With(requirement, count, requirement.Infinity(count), $"{Localization.Keys.CommonItemTooltips}.{extra}");
    }

    public FullInfinity this[IInfinity infinity] => _infinities[infinity];
    public FullInfinity Mixed { get; private set; }
    public FullInfinity EffectiveInfinity(IInfinity group) {
        if (!group.Enabled) return FullInfinity.None;
        FullInfinity fullInfinity = this[group];
        return fullInfinity.Requirement.IsNone || UsedInfinities.Contains(group) ? fullInfinity : Mixed;
    }

    public HashSet<IInfinity> UsedInfinities { get; private set; }

    private readonly Dictionary<IInfinity, FullInfinity> _infinities;

}

public sealed class ItemDisplay {

    public ItemDisplay() {
        DisplayedInfinities = new();
        InfinitiesByGroup = new();
        _infinities = new();
    }

    public FullInfinity this[IInfinity infinity] => _infinities[infinity];

    public void Add(IInfinity infinity, FullInfinity display, InfinityVisibility visibility) {
        void Add(IInfinity infinity){
            DisplayedInfinities.Add(infinity);
            if (InfinitiesByGroup.Count == 0 || InfinitiesByGroup[^1][0].Group != infinity.Group) InfinitiesByGroup.Add(new());
            InfinitiesByGroup[^1].Add(infinity);
        }
        
        _infinities[infinity] = display;
        switch (visibility) {
        case InfinityVisibility.Normal:
            if(!_exclusiveDisplay) Add(infinity);
            break;
        case InfinityVisibility.Exclusive:
            if (!_exclusiveDisplay) {
                DisplayedInfinities.Clear();
                InfinitiesByGroup.Clear();
            }
            Add(infinity);
            _exclusiveDisplay = true;
            break;
        }
    }

    private bool _exclusiveDisplay;

    private readonly Dictionary<IInfinity, FullInfinity> _infinities;
    public List<IInfinity> DisplayedInfinities { get; private set; }
    public List<List<IInfinity>> InfinitiesByGroup { get; private set; }
}