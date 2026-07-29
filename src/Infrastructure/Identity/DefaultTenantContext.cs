using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Infrastructure.Persistence.Seed;

namespace AiKocStudio.Infrastructure.Identity;

/// <summary>
/// MVP is single-tenant: always resolves to the seeded default tenant regardless
/// of the current user/request. This is the swap-point for real multi-tenant
/// resolution (e.g. reading TenantId from the JWT or a subdomain) once the
/// product needs it — nothing else in the codebase needs to change to flip it on,
/// since every query already goes through the "TenantFilter" this context feeds.
/// </summary>
public class DefaultTenantContext : ITenantContext
{
    public Guid TenantId => DefaultTenantSeed.TenantId;
}
