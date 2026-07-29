using AiKocStudio.Domain.Entities;

namespace AiKocStudio.Infrastructure.Persistence.Seed;

/// <summary>
/// MVP is single-tenant: every row belongs to this one well-known tenant.
/// Seeded as a real row (not NULL) so query filters and future migrations to
/// real multi-tenancy don't need a backfill (see docs/research on tenant filters).
/// </summary>
public static class DefaultTenantSeed
{
    public static readonly Guid TenantId = new("00000000-0000-0000-0000-000000000001");

    public static Tenant Data => new()
    {
        Id = TenantId,
        Name = "Default",
        IsDefault = true,
    };
}
