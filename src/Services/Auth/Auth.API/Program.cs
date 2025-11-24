using Auth.API.Extensions;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Configuration Settings

// Configure authentication and OAuth settings
var (authSettings, oauthSettings) = builder.Services.ConfigureAuthSettings(builder.Configuration);

#endregion

#region Redis Configuration

// Configure Redis connection using Infrastructure extension
builder.Services.AddRedisInfrastructure(builder.Configuration);

#endregion

#region Auth Services

// Add authentication services with resilience policies
builder.Services.AddAuthServices();

#endregion

#region Controllers & Swagger

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger documentation
builder.Services.AddAuthSwagger();

#endregion

#region CORS

// Add CORS policy using Infrastructure extension
builder.Services.AddCorsForDevelopment("AllowAll");

#endregion

#region Health Checks

// Add health checks for Auth API using Infrastructure extension
builder.Services.AddHealthCheckWithRedis(authSettings.ConnectionStrings, "redis")
                .AddHealthCheckConfiguration();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Apply CORS using Infrastructure extension
app.UseCorsConfiguration("AllowAll");

// Configure Swagger UI for development
app.UseAuthSwagger(app.Environment);

// Routing
app.UseRouting();

// Logging Context Middleware - Thêm correlation ID và username vào logs
app.UseLoggingContext();

// Map controllers
app.MapControllers();

// Map health check endpoint using Infrastructure extension
app.UseHealthCheckConfiguration();

#endregion

app.Run();
