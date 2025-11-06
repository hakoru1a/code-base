using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using ApiGateway.Configurations;
using ApiGateway.Services;
using ApiGateway.Middlewares;
using ApiGateway.Handlers;
using StackExchange.Redis;
using Contracts.Common.Interface;
using Infrastructure.Common.Repository;
using Polly;

var builder = WebApplication.CreateBuilder(args);

#region Configuration Settings

// Load configuration settings - sử dụng BffSettings kế thừa từ CacheSettings có sẵn
var bffSettings = builder.Configuration
    .GetSection(BffSettings.SectionName)
    .Get<BffSettings>() ?? new BffSettings();

var oauthSettings = builder.Configuration
    .GetSection(OAuthSettings.SectionName)
    .Get<OAuthSettings>() ?? throw new InvalidOperationException("OAuth settings not configured");

// Register settings as singletons
builder.Services.AddSingleton(bffSettings);
builder.Services.AddSingleton(oauthSettings);

#endregion

#region Redis Configuration - Sử dụng Infrastructure có sẵn

// Cấu hình Redis connection
// Redis được dùng để:
// 1. Lưu user sessions (session_id -> UserSession)
// 2. Lưu PKCE data (state -> PkceData)
// 3. Share sessions giữa multiple gateway instances (scalability)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(bffSettings.ConnectionStrings);
    configuration.AbortOnConnectFail = false; // Không crash app nếu Redis down
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;

    return ConnectionMultiplexer.Connect(configuration);
});

// Register IRedisRepository từ Infrastructure có sẵn
builder.Services.AddScoped<IRedisRepository, RedisRepository>();

#endregion

#region BFF Services

// HttpContextAccessor - cần thiết để access HttpContext trong DelegatingHandler
builder.Services.AddHttpContextAccessor();

// PKCE Service - handle PKCE flow security
builder.Services.AddScoped<IPkceService, PkceService>();

// Session Manager - quản lý user sessions trong Redis
builder.Services.AddScoped<ISessionManager, SessionManager>();

// OAuth Client - communicate với Keycloak Token Endpoint
builder.Services.AddHttpClient<IOAuthClient, OAuthClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(Polly.Extensions.Http.HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))); // Exponential backoff

#endregion

#region Ocelot Configuration

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot services với custom DelegatingHandler
builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<TokenDelegatingHandler>(global: true); // Apply to all routes

// Register TokenDelegatingHandler
builder.Services.AddTransient<TokenDelegatingHandler>();

#endregion

#region Controllers & Swagger

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gateway - BFF Pattern",
        Version = "v1",
        Description = @"
            API Gateway với Backend-for-Frontend (BFF) Pattern
            
            Authentication Flow:
            1. GET /auth/login - Khởi tạo OAuth login flow
            2. User login tại Keycloak
            3. GET /auth/signin-oidc - Callback xử lý tokens
            4. Session được tạo và lưu trong Redis
            5. HttpOnly cookie được set với session_id
            6. Các API calls tự động có Bearer token từ session
            
            Security Features:
            - OAuth 2.0 Authorization Code Flow + PKCE
            - Tokens được lưu backend (Redis), không expose ra browser
            - HttpOnly cookies cho session management
            - Automatic token refresh
            - CSRF protection với state parameter
        "
    });

    // Add security definition (informational only)
    c.AddSecurityDefinition("Session", new OpenApiSecurityScheme
    {
        Description = "Session-based authentication using HttpOnly cookies",
        Name = "session_id",
        In = ParameterLocation.Cookie,
        Type = SecuritySchemeType.ApiKey
    });
});

#endregion

#region CORS

// CORS configuration
// QUAN TRỌNG: Phải configure credentials = true để browser gửi cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(oauthSettings.WebAppUrl) // Chỉ allow webapp origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Cho phép browser gửi cookies
    });
});

#endregion

#region Health Checks

builder.Services.AddHealthChecks()
    .AddRedis(bffSettings.ConnectionStrings, name: "redis", tags: new[] { "cache" });

#endregion

var app = builder.Build();

#region Middleware Pipeline

// Apply CORS - PHẢI đặt trước các middleware khác
app.UseCors("AllowWebApp");

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

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.SwaggerEndpoint("http://localhost:5239/swagger/v1/swagger.json", "Base API");
        c.SwaggerEndpoint("http://localhost:5027/swagger/v1/swagger.json", "Generate API");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.EnableValidator();
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = "swagger";
    });
}

// Routing
app.UseRouting();

// Session Validation Middleware
// Middleware này sẽ:
// 1. Validate session cookie
// 2. Load session từ Redis
// 3. Refresh token nếu cần
// 4. Set AccessToken vào HttpContext.Items
app.UseSessionValidation();

// Map endpoints (Auth Controller)
// QUAN TRỌNG: app.MapControllers() sẽ tự động gọi app.UseEndpoints()
// Controller routes sẽ được xử lý trước Ocelot middleware
app.MapControllers();

// Health check endpoint
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
                duration = e.Value.Duration.ToString(),
                tags = e.Value.Tags
            })
        });
        await context.Response.WriteAsync(result);
    }
});

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
