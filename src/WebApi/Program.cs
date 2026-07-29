using AiKocStudio.Infrastructure;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");

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

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnectionString, name: "postgres")
    .AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>(), name: "redis");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

// Anonymous access is acceptable only because this is gated to Development.
// Phase 2 adds real auth; a production dashboard needs an IDashboardAuthorizationFilter
// (or must stay off the public network entirely) before this ever runs non-locally.
if (app.Environment.IsDevelopment())
{
    app.MapHangfireDashboard("/hangfire");
}

app.Run();

public partial class Program;
