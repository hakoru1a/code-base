using Auth.Application.Interfaces;
using Auth.Domain.Configurations;
using Auth.Infrastructure.Services;
using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using Microsoft.OpenApi.Models;
using Polly;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

#region Configuration Settings

var authSettings = builder.Configuration
    .GetSection(AuthSettings.SectionName)
    .Get<AuthSettings>() ?? new AuthSettings();

var oauthSettings = builder.Configuration
    .GetSection(OAuthSettings.SectionName)
    .Get<OAuthSettings>() ?? throw new InvalidOperationException("OAuth settings not configured");

builder.Services.AddSingleton(authSettings);
builder.Services.AddSingleton(oauthSettings);

#endregion

#region Redis Configuration

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(authSettings.ConnectionStrings);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IRedisRepository, RedisRepository>();

#endregion

#region Auth Services

builder.Services.AddScoped<IPkceService, PkceService>();
builder.Services.AddScoped<ISessionManager, SessionManager>();

builder.Services.AddHttpClient<IOAuthClient, OAuthClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(Polly.Extensions.Http.HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

#endregion

#region Controllers & Swagger

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Authentication service xử lý OAuth 2.0/OIDC với Keycloak"
    });
});

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

#endregion

#region Health Checks

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("redis", () =>
    {
        try
        {
            var redis = builder.Services.BuildServiceProvider().GetService<IConnectionMultiplexer>();
            if (redis?.IsConnected == true)
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Redis is connected");
            else
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis is not connected");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Redis health check failed: {ex.Message}");
        }
    }, tags: new[] { "cache" });

#endregion

var app = builder.Build();

#region Middleware Pipeline

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});

#endregion

app.Run();
