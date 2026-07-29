using System.Reflection;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Common;
using AiKocStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Every entity implementing ITenantScoped gets the same tenant query filter
        // applied here — this is the single place new entities (Phase 3+) get tenant
        // isolation "for free" by extending BaseAuditableEntity.
        //
        // EF Core 8 allows only ONE HasQueryFilter call per entity (a second call
        // silently replaces the first — named/combinable filters aren't available
        // until EF Core 10). If a later phase adds e.g. soft-delete, it MUST be
        // AND-ed into this same lambda (e.g. `e.TenantId == tenantId && !e.IsDeleted`)
        // rather than registered as a separate filter, or it will silently disable
        // tenant isolation for that entity.
        var applyTenantFilterMethod = typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                applyTenantFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenantContext.TenantId);
    }
}
