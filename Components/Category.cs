
using System;
using Microsoft.CodeAnalysis;

namespace SPIC.Components;

public sealed class Category<TConsumable, TCategory> : Component<Infinity<TConsumable>>, Endpoints.ICategoryAccessor<TConsumable, TCategory> where TCategory : struct, Enum {
    
    public Category(Func<TCategory, Optional<Requirement>> getRequirement, ProviderList<TConsumable, TCategory>.ProviderFn? getCategory = null) {
        GetRequirement = getRequirement;
        _getCategory = getCategory;
    }

    public override void Bind() {
        Endpoints.GetRequirement(Infinity).Providers.Add(c => GetRequirement(InfinityManager.GetCategory(c, this)));
        if (_getCategory is not null) Endpoints.GetCategory(this).Providers.Add(_getCategory);
    }

    public Func<TCategory, Optional<Requirement>> GetRequirement { get; private set; }
    private readonly ProviderList<TConsumable, TCategory>.ProviderFn? _getCategory;
}
