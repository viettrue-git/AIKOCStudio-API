using AiKocStudio.Domain.Entities;
using AiKocStudio.Domain.Enums;
using AiKocStudio.Infrastructure.IntegrationTests.Common;
using AiKocStudio.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Infrastructure.IntegrationTests.Persistence;

/// <summary>
/// Proves the tenant query-filter mechanics in ApplicationDbContext actually
/// isolate rows by TenantId, even though MVP always resolves a single default
/// tenant in production (see DefaultTenantContext) — this is what makes flipping
/// on real multi-tenancy later safe to trust.
///
/// Uses EF Core's InMemory provider since no live Postgres is available in this
/// dev environment (Docker isn't installed) — InMemory does honor HasQueryFilter,
/// so the mechanics under test are exercised faithfully even without a real DB.
/// </summary>
public class QueryFilterTests
{
    private static ApplicationDbContext CreateContext(string databaseName, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options, new TestTenantContext(tenantId));
    }

    [Fact]
    public async Task Users_AreIsolatedByTenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        using (var writerContext = CreateContext(databaseName, tenant1Id))
        {
            writerContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant1Id,
                Email = "tenant1-user@example.com",
                PasswordHash = "hashed",
                DisplayName = "Tenant 1 User",
                Role = UserRole.Member,
                IsActive = true,
            });
            await writerContext.SaveChangesAsync();
        }

        using (var otherTenantReaderContext = CreateContext(databaseName, tenant2Id))
        {
            var visibleToOtherTenant = await otherTenantReaderContext.Users.ToListAsync();
            visibleToOtherTenant.Should().BeEmpty("tenant 2's query filter must not see tenant 1's rows");
        }

        using (var sameTenantReaderContext = CreateContext(databaseName, tenant1Id))
        {
            var visibleToSameTenant = await sameTenantReaderContext.Users.ToListAsync();
            visibleToSameTenant.Should().ContainSingle(u => u.Email == "tenant1-user@example.com");
        }
    }
}
