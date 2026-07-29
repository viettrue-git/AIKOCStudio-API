namespace AiKocStudio.Domain.Common;

/// <summary>
/// Marks an entity as belonging to a tenant. EF Core applies a global query
/// filter to every entity implementing this interface (see
/// Infrastructure/Persistence/ApplicationDbContext.cs).
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
