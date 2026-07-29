namespace AiKocStudio.Domain.Common;

/// <summary>
/// Marks an entity as soft-deletable. Combined with ITenantScoped (when an
/// entity implements both) into a single query filter in ApplicationDbContext —
/// EF Core 8 allows only one HasQueryFilter per entity, so this can never be
/// registered as an independent second filter (see ApplicationDbContext.cs).
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
