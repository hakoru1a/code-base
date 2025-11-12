using Auth.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Configuration Settings

// Configure authentication and OAuth settings
var (authSettings, oauthSettings) = builder.Services.ConfigureAuthSettings(builder.Configuration);

#endregion

#region Redis Configuration

// Configure Redis connection and repository
builder.Services.AddRedisConfiguration(authSettings);

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

// Add CORS policy that allows all origins
builder.Services.AddAuthCors();

#endregion

#region Health Checks

// Add health checks for Auth API
builder.Services.AddAuthHealthChecks();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Apply CORS
app.UseAuthCors();

// Configure Swagger UI for development
app.UseAuthSwagger(app.Environment);

// Routing
app.UseRouting();

// Map controllers
app.MapControllers();

// Map health check endpoint
app.MapAuthHealthChecks();

#endregion

app.Run();
