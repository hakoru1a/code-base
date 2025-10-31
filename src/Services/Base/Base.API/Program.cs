using Base.API.Extensions;
using Base.Application;
using Base.Application.Feature.Product.Policies;
using Base.Infrastructure;
using Common.Logging;
using Infrastructure.Extensions;
using Serilog;
using Shared.Identity;


try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.Host.AddAppConfigurations();

    // Add Keycloak Authentication (RBAC at Gateway, but also validated at Service level)
    builder.Services.AddKeycloakAuthentication(builder.Configuration);
    builder.Services.AddKeycloakAuthorization();

    // Add Policy-Based Authorization (PBAC at Service level)
    builder.Services.AddPolicyBasedAuthorization(policies =>
    {
        policies.AddPolicy<ProductViewPolicy>(PolicyNames.Pbac.Product.View);
        policies.AddPolicy<ProductCreatePolicy>(PolicyNames.Pbac.Product.Create);
        policies.AddPolicy<ProductUpdatePolicy>(PolicyNames.Pbac.Product.Update);
        policies.AddPolicy<ProductListFilterPolicy>(PolicyNames.Pbac.Product.ListFilter);
    });

    builder.Services.AddInfrastructure(builder.Configuration)
                    .AddConfigurationSettings(builder.Configuration)
                    .AddApplicationServices()
                    .AddHealthCheckServices();

    builder.Services.AddControllers();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Enable CORS early in pipeline
    app.UseCors("AllowAll");

    // Add Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Add Policy-Based Authorization Middleware (PBAC)
    app.UsePolicyAuthorization();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        // Change the Swagger endpoint
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });

    app.UseHttpsRedirection();

    app.MapControllers();

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
    // X? ly rieng cho HostAbortedException
    Log.Warning("Host was aborted during startup: {Message}", ex.Message);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandle Exception");
}
finally
{
    Log.Information("Shut down Order API complete");
    Log.CloseAndFlush();
}
