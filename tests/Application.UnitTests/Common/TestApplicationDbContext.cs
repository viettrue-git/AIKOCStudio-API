using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.UnitTests.Common;

/// <summary>
/// EF Core InMemory-backed stand-in for ApplicationDbContext (which lives in
/// Infrastructure, not referenced by this test project). No query filters here —
/// these tests exercise command/handler logic, not tenant-isolation mechanics
/// (that's covered by Infrastructure.IntegrationTests/QueryFilterTests).
/// </summary>
public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Product> Products => Set<Product>();

    public static TestApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }
}
