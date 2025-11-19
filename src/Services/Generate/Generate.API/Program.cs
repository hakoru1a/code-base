using Generate.API.Extensions;
using Generate.Application;
using Generate.Application.Features.Category.Policies;
using Generate.Application.Features.Product.Policies;
using Generate.Application.Features.Order.Policies;
using Generate.Infrastructure;
using Common.Logging;
using Infrastructure.Extensions;
using Serilog;
using Shared.Identity;



try
{
    var builder = WebApplication.CreateBuilder(args);
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
    Log.Information("Starting Generate API");

    // Add Serilog Configuration
    builder.Host.UseSerilog(SeriLogger.Configure);

    // Add Configuration
    builder.Host.AddConfiguration();

    // Add Keycloak Authentication (RBAC at Gateway, but also validated at Service level)
    builder.Services.AddKeycloakAuthentication(builder.Configuration);
    builder.Services.AddKeycloakAuthorization();

    // Add Policy-Based Authorization (PBAC at Service level)
    builder.Services.AddPolicyBasedAuthorization(policies =>
    {
        // Category Policies
        policies.AddPolicy<CategoryViewPolicy>(PolicyNames.Pbac.Category.View);
        policies.AddPolicy<CategoryCreatePolicy>(PolicyNames.Pbac.Category.Create);
        policies.AddPolicy<CategoryUpdatePolicy>(PolicyNames.Pbac.Category.Update);
        policies.AddPolicy<CategoryDeletePolicy>(PolicyNames.Pbac.Category.Delete);

        // Product Policies
        policies.AddPolicy<ProductViewPolicy>(PolicyNames.Pbac.Product.View);
        policies.AddPolicy<ProductCreatePolicy>(PolicyNames.Pbac.Product.Create);
        policies.AddPolicy<ProductUpdatePolicy>(PolicyNames.Pbac.Product.Update);
        policies.AddPolicy<ProductDeletePolicy>(PolicyNames.Pbac.Product.Delete);

        // Order Policies
        policies.AddPolicy<OrderViewPolicy>(PolicyNames.Pbac.Order.View);
        policies.AddPolicy<OrderCreatePolicy>(PolicyNames.Pbac.Order.Create);
        policies.AddPolicy<OrderUpdatePolicy>(PolicyNames.Pbac.Order.Update);
        policies.AddPolicy<OrderDeletePolicy>(PolicyNames.Pbac.Order.Delete);
    });

    // Add services to the container
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add Application Services (MediatR, Mapster, FluentValidation)
    builder.Services.AddApplicationServices();

    // Add Infrastructure Services (Database, Repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

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
