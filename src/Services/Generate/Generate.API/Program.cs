using Generate.API.Extensions;
using Generate.Application.Features.Categories.Policies;
using Generate.Application.Features.Products.Policies;
using Generate.Application.Features.Orders.Policies;
using Generate.Infrastructure;
using Common.Logging;
using Infrastructure.Extensions;
using Serilog;
using Shared.Identity;
using DotNetEnv;



try
{
    // Load .env file before creating builder
    Env.Load();

    var builder = WebApplication.CreateBuilder(args);
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
    Log.Information("Starting Generate API");

    // Add Serilog Configuration
    builder.Host.UseSerilog(SeriLogger.Configure);

    // Substitute environment variables in configuration (${VARIABLE} syntax)
    builder.Configuration.SubstituteEnvironmentVariables();

    // Add services to the container
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add Infrastructure Services (Database, Repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add Authentication and Authorization
    builder.Services.AddAuthenticationConfiguration(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Use Infrastructure middleware (includes Authentication and Authorization)
    app.UseInfrastructure();

    // Add Policy-Based Authorization Middleware (PBAC)
    app.UsePolicyAuthorization();

    // Map Health Check endpoint
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString()
                }),
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.Run();
}
catch (HostAbortedException ex)
{
    // Handle HostAbortedException separately
    Log.Warning("Host was aborted during startup: {Message}", ex.Message);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    Log.Information("Shut down Generate API complete");
    Log.CloseAndFlush();
}
