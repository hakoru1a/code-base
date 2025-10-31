using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Keycloak Authentication (RBAC at Gateway level)
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddKeycloakAuthorization();

// Add Ocelot services
builder.Services.AddOcelot(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "API Gateway cho Base và Generate Services"
    });
});

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

app.UseCors("AllowAll");

// Redirect root to Swagger UI - must be before Ocelot middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }
    await next();
});

// Add Routing middleware - must be before endpoints
app.UseRouting();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("https://localhost:44315/swagger/v1/swagger.json", "Base API");
        c.SwaggerEndpoint("https://localhost:44319/swagger/v1/swagger.json", "Generate API 1");
        c.SwaggerEndpoint("https://localhost:44319/swagger/v2/swagger.json", "Generate API 2");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.EnableValidator();
    });
}
else
{
    // Enable Swagger UI in non-development for quick access to docs
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = "swagger";
    });
}

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints BEFORE Ocelot middleware
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
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Use Ocelot middleware - should be after endpoint mappings
await app.UseOcelot();

app.Run();
