using Product.API.Extensions;
using Product.Application;
using Product.Infrastructure;
using Common.Logging;
using Infrastructure.Extensions;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
    Log.Information("Starting Product API");

    // Add Serilog Configuration
    builder.Host.UseSerilog(SeriLogger.Configure);

    // Add Configuration
    builder.Host.AddConfiguration();

    // Add services to the container
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add Application Services (MediatR, AutoMapper, FluentValidation)
    builder.Services.AddApplicationServices();

    // Add Infrastructure Services (Database, Repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Use Infrastructure middleware
    app.UseInfrastructure();

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
    Log.Information("Shut down Product API complete");
    Log.CloseAndFlush();
}