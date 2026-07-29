using System.Text;
using AiKocStudio.Application;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Constants;
using AiKocStudio.Domain.Entities;
using AiKocStudio.Domain.Enums;
using AiKocStudio.Infrastructure;
using AiKocStudio.Infrastructure.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AiKocStudio.WebApi.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(postgresConnectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("Jwt:Secret is not configured.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnectionString, name: "postgres")
    .AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>(), name: "redis");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = AiKocStudio.Infrastructure.Identity.JwtClaimTypes.Role,
            NameClaimType = AiKocStudio.Infrastructure.Identity.JwtClaimTypes.UserId,
        };
    });

// Every controller requires an authenticated caller by default (explicit allow-list
// via [AllowAnonymous] on specific actions, not a deny-list of protected ones).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    // In Development, anyone reaching the box can open it (convenience).
    // Everywhere else it requires a valid JWT with the Admin role — same
    // auth this phase just wired up for the rest of the API.
    Authorization = app.Environment.IsDevelopment()
        ? [new AllowAllDashboardAuthorizationFilter()]
        : [new AdminOnlyDashboardAuthorizationFilter()],
});

await SeedFirstAdminUserAsync(app);

app.Run();

static async Task SeedFirstAdminUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // MigrateAsync only works against a relational provider — functional tests
    // swap in EF Core InMemory (no Docker/Postgres in this dev environment),
    // which needs EnsureCreatedAsync instead.
    if (context.Database.IsRelational())
    {
        await context.Database.MigrateAsync();
    }
    else
    {
        await context.Database.EnsureCreatedAsync();
    }

    if (await context.Users.AnyAsync())
    {
        return;
    }

    var email = app.Configuration["Admin:Email"];
    var password = app.Configuration["Admin:Password"];
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        logger.LogWarning(
            "No users exist and Admin:Email/Admin:Password are not configured — skipping first-admin seed. " +
            "Set both in config (env vars in production) and restart to bootstrap the first Admin user.");
        return;
    }

    var admin = new User
    {
        Id = Guid.NewGuid(),
        Email = email,
        DisplayName = "Admin",
        Role = UserRole.Admin,
        IsActive = true,
    };
    admin.PasswordHash = identityService.HashPassword(admin, password);

    context.Users.Add(admin);
    await context.SaveChangesAsync();

    logger.LogWarning(
        "Seeded first Admin user {Email} from Admin:Email/Admin:Password config — change this password immediately.",
        email);
}

file class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}

file class AdminOnlyDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(Roles.Admin);
    }
}

public partial class Program;
