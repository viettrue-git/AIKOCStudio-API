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
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // EF Core 8 allows only ONE HasQueryFilter call per entity (a second call
        // silently replaces the first — named/combinable filters aren't available
        // until EF Core 10). So an entity implementing BOTH ITenantScoped and
        // ISoftDelete (e.g. Persona/Product) must get ONE filter with both
        // conditions AND-ed together, not two separate registrations.
        var applyTenantAndSoftDeleteFilterMethod = typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplyTenantAndSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var applyTenantFilterMethod = typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var applySoftDeleteFilterMethod = typeof(ApplicationDbContext)
            .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isTenantScoped = typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType);
            var isSoftDelete = typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType);

            if (isTenantScoped && isSoftDelete)
            {
                applyTenantAndSoftDeleteFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
            else if (isTenantScoped)
            {
                applyTenantFilterMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, [modelBuilder]);
            }
            else if (isSoftDelete)
            {
                // No entity is soft-delete-only today (Persona/Product are both also
                // tenant-scoped) — this branch exists so a future one doesn't silently
                // get zero query filter at all.
                applySoftDeleteFilterMethod
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

    private void ApplyTenantAndSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenantContext.TenantId && !e.IsDeleted);
    }

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
