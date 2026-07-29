using AiKocStudio.Infrastructure.Persistence;
using AiKocStudio.Infrastructure.Persistence.Interceptors;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace AiKocStudio.Application.FunctionalTests;

/// <summary>
/// Swaps the real Postgres-backed ApplicationDbContext for EF Core InMemory —
/// Docker isn't available in this dev environment, so Testcontainers-based
/// Postgres isn't an option (per the Phase 3 plan's explicit fallback: "Testcontainers
/// Postgres or in-memory provider"). Everything else (JWT auth, first-admin seed,
/// MediatR pipeline, controllers) runs exactly as it does in production.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not set by default in this environment (no ASPNETCORE_ENVIRONMENT var),
        // which would otherwise fall back to Production and skip
        // appsettings.Development.json — leaving Jwt:Secret/Admin:Email/Password
        // empty and failing Program.cs's own startup guards.
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (optionsDescriptor is not null)
            {
                services.Remove(optionsDescriptor);
            }

            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (contextDescriptor is not null)
            {
                services.Remove(contextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
                options.UseInMemoryDatabase(_databaseName)
                       .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

            // No live Redis in this dev environment either — drop the
            // Redis-backed multiplexer and every Hangfire service AddInfrastructureServices
            // registered, then re-register Hangfire against in-memory storage instead.
            // Only the dashboard's startup check (IGlobalConfiguration must exist) matters
            // for these tests; AddHangfireServer isn't needed since no test runs a background job.
            services.RemoveAll<IConnectionMultiplexer>();

            var hangfireServiceTypes = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("Hangfire", StringComparison.Ordinal) == true)
                .Select(d => d.ServiceType)
                .Distinct()
                .ToList();
            foreach (var serviceType in hangfireServiceTypes)
            {
                services.RemoveAll(serviceType);
            }

            services.AddHangfire(config => config.UseMemoryStorage());
        });
    }
}
