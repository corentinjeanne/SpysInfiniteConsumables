
using System;
using Microsoft.CodeAnalysis;

namespace SPIC.Default.Components;

public sealed class Category<TConsumable, TCategory> : Component<Infinity<TConsumable>>, ICategoryAccessor<TConsumable, TCategory> where TCategory : struct, Enum {
    
    public Category(Func<TCategory, Optional<Requirement>> getRequirement, IEndpoint<TConsumable, TCategory>.ProviderFn? getCategory = null) {
        GetRequirement = getRequirement;
        _getCategory = getCategory;
    }

    public override void Bind() {
        Endpoints.GetRequirement(Infinity).AddProvider(c => GetRequirement(InfinityManager.GetCategory(c, this)));
        if (_getCategory is not null) Endpoints.GetCategory(this).AddProvider(_getCategory);
    }

    public Func<TCategory, Optional<Requirement>> GetRequirement { get; private set; }
    private readonly IEndpoint<TConsumable, TCategory>.ProviderFn? _getCategory;
}
