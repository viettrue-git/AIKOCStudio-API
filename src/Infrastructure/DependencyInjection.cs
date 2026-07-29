using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Infrastructure.Identity;
using AiKocStudio.Infrastructure.Persistence;
using AiKocStudio.Infrastructure.Persistence.Interceptors;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AiKocStudio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Connection string 'Redis' is not configured.");
        }

        var postgresConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddHangfire((sp, config) => config
            .UseRedisStorage(sp.GetRequiredService<IConnectionMultiplexer>()));
        services.AddHangfireServer();

        services.AddHttpContextAccessor();

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(postgresConnectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ITenantContext, DefaultTenantContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IIdentityService, PasswordHasherWrapper>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
