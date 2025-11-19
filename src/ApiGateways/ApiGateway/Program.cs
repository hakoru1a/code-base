using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using ApiGateway.Middlewares;
using ApiGateway.Handlers;
using ApiGateway.Configurations;
using Infrastructure.Extensions;
using ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration file FIRST - before any other configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

#region Configuration Settings

// Configure services options
builder.Services.Configure<ServicesOptions>(
    builder.Configuration.GetSection(ServicesOptions.SectionName));

// Configure OAuth options
builder.Services.Configure<OAuthOptions>(
    builder.Configuration.GetSection(OAuthOptions.SectionName));

// Get options for immediate use using Infrastructure extension
var servicesOptions = builder.Configuration.GetOptions<ServicesOptions>(ServicesOptions.SectionName);
var oAuthOptions = builder.Configuration.GetOptions<OAuthOptions>(OAuthOptions.SectionName);

#endregion

#region HTTP Client

// HttpContextAccessor - cần thiết để access HttpContext trong DelegatingHandler
builder.Services.AddHttpContextAccessor();

// Configure named HttpClients with logging and resilience policies
builder.Services.AddConfiguredHttpClients(servicesOptions);

#endregion

#region Authentication & Authorization

// Add Keycloak JWT Authentication for RBAC
// This will validate JWT tokens and extract roles/claims from access_token in session
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Add Keycloak Authorization Policies (RBAC)
// This registers policies like AdminOnly, ManagerOrAdmin, etc.
builder.Services.AddKeycloakAuthorization();

#endregion

#region Ocelot Configuration

// Add Ocelot services với custom DelegatingHandler
builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<TokenDelegatingHandler>(global: true); // Apply to all routes

// Register TokenDelegatingHandler
builder.Services.AddTransient<TokenDelegatingHandler>();

#endregion

#region Controllers & Swagger

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with OpenAPI documentation
builder.Services.AddGatewaySwagger();

#endregion

#region CORS

// Add CORS policy for web application
builder.Services.AddGatewayCors(oAuthOptions);

#endregion

#region Health Checks

// Add health checks using Infrastructure extension
builder.Services.AddHealthCheckConfiguration();

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Apply CORS - PHẢI đặt trước các middleware khác
app.UseGatewayCors();

// Redirect root to Swagger UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }
    await next();
});

// Configure Swagger UI with downstream services
app.UseGatewaySwaggerUI(app.Environment, servicesOptions);

// Routing
app.UseRouting();

// Session Validation Middleware
// Middleware này sẽ:
// 1. Validate session cookie
// 2. Load session từ Redis
// 3. Refresh token nếu cần
// 4. Set AccessToken vào HttpContext.Items
// 5. Parse JWT và set HttpContext.User (for RBAC)
app.UseSessionValidation();

// Authentication & Authorization Middleware
// QUAN TRỌNG: Phải đặt sau UseSessionValidation để có User context
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints (Auth Controller)
// QUAN TRỌNG: app.MapControllers() sẽ tự động gọi app.UseEndpoints()
// Controller routes sẽ được xử lý trước Ocelot middleware
app.MapControllers();

// Map health check endpoint using Infrastructure extension
app.UseHealthCheckConfiguration();

// Map development-only endpoints like _whoami
app.MapDevelopmentEndpoints(app.Environment);

// Endpoint routing - PHẢI gọi để đảm bảo controller routes được map
// app.MapControllers() đã tự động gọi UseEndpoints(), nhưng gọi thêm để chắc chắn
app.UseEndpoints(endpoints =>
{
    // Controllers đã được map ở app.MapControllers() ở trên
    // Endpoints sẽ được xử lý bởi controller routing trước khi đến Ocelot
});

// Ocelot Middleware - PHẢI đặt cuối cùng
// Ocelot sẽ:
// 1. Match incoming requests với routes trong ocelot.json
// 2. Apply TokenDelegatingHandler (add Bearer token)
// 3. Forward requests tới downstream services
// LƯU Ý: Ocelot chỉ xử lý requests KHÔNG match với controller routes
// Controller routes (/auth/*, /swagger/*, /health) sẽ được xử lý trước Ocelot
await app.UseOcelot();

#endregion

app.Run();
