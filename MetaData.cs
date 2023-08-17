using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace SPIC;

public sealed class GroupInfinity {

    internal GroupInfinity() {}

    internal void Add(IInfinity infinity, FullInfinity fullInfinity, bool used) {
        if (used) _infinities[infinity] = fullInfinity;
        else if(!fullInfinity.Requirement.IsNone) _unused.Add(infinity);
    }
    internal void AddMixed(Requirement? custom = null) {
        long count = long.MaxValue;
        long reqCount = 0; float reqMult = 1;
        foreach(FullInfinity fullInfinity in _infinities.Values){
            count = Math.Min(count, fullInfinity.Count);
            reqCount = Math.Max(reqCount, fullInfinity.Requirement.Count);
            reqMult = Math.Min(reqMult, fullInfinity.Requirement.Multiplier);
        }
        string extra;
        Requirement requirement;
        if(custom.HasValue){
            extra = "Custom";
            requirement = custom.Value;
            foreach (IInfinity infinity in _infinities.Keys) _unused.Add(infinity);
            _infinities.Clear();
        } else {
            extra = "Mixed";
            requirement = new(reqCount, reqMult);
        }
        Mixed = FullInfinity.With(requirement, count, requirement.Infinity(count), $"{Localization.Keys.CommonItemTooltips}.{extra}");
    }

    public FullInfinity EffectiveInfinity(IInfinity infinity) {
        if(_infinities.TryGetValue(infinity, out FullInfinity? effective)) return effective;
        if(_unused.Contains(infinity)) return Mixed;
        return FullInfinity.None;
    }

    public ReadOnlyDictionary<IInfinity, FullInfinity> UsedInfinities => new(_infinities);
    public IReadOnlySet<IInfinity> UnusedInfinities => _unused;
    public FullInfinity Mixed { get; private set; } = FullInfinity.None;

    internal readonly Dictionary<IInfinity, FullInfinity> _infinities = new();
    private readonly HashSet<IInfinity> _unused = new();
}

public sealed class ItemDisplay {

    internal ItemDisplay() {}

    internal void Add(IInfinity infinity, int displayed, FullInfinity display, InfinityVisibility visibility) {
        switch (visibility) {
        case InfinityVisibility.Normal:
            if (ExclusiveDisplay) return;
            break;
        case InfinityVisibility.Exclusive:
            if (!ExclusiveDisplay) {
                _infinities.Clear();
                ExclusiveDisplay = true;
            }
            break;
        }
        if(_infinities.Count == 0 || _infinities[^1].infinity.Group != infinity.Group) _groups.Add(_infinities.Count);
        _infinities.Add((infinity, displayed, display));
    }

    public ReadOnlySpan<(IInfinity infinity, int displayed, FullInfinity display)> DisplayedInfinities => CollectionsMarshal.AsSpan(_infinities);
    public ReadOnlySpan<(IInfinity infinity, int displayed, FullInfinity display)> InfinitiesByGroups(int index)
        => index >= Groups-1 ? CollectionsMarshal.AsSpan(_infinities)[_groups[index]..] : CollectionsMarshal.AsSpan(_infinities)[_groups[index].._groups[index+1]];

    public int Groups => _groups.Count;

    public bool ExclusiveDisplay { get; private set; }

    private readonly List<(IInfinity infinity, int displayed, FullInfinity display)> _infinities = new();
    private readonly List<int> _groups = new();
}
