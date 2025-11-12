using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.OpenApi.Models;
using ApiGateway.Middlewares;
using ApiGateway.Handlers;
using Microsoft.AspNetCore.Authentication;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Configuration Settings

// Gateway không cần BFF settings nữa vì logic đã chuyển sang Auth service
// Chỉ cần cấu hình URL của Auth service
builder.Services.Configure<Dictionary<string, string>>(
    builder.Configuration.GetSection("Services"));

#endregion

#region HTTP Client

// HttpContextAccessor - cần thiết để access HttpContext trong DelegatingHandler
builder.Services.AddHttpContextAccessor();

// HttpClientFactory để gọi Auth service
builder.Services.AddHttpClient();

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
        Title = "API Gateway",
        Version = "v1",
        Description = @"
            API Gateway - Simple Routing & Session Management
            
            Architecture:
            - Gateway chỉ đảm nhận routing và session validation đơn giản
            - Toàn bộ OAuth 2.0/OIDC logic được xử lý bởi Auth Service
            - RBAC/PBAC được thực thi tại Gateway và Backend Services
            
            Authentication Flow:
            1. GET /auth/login → Proxy tới Auth Service
            2. Auth Service xử lý OAuth 2.0 + PKCE với Keycloak
            3. GET /auth/signin-oidc → Proxy tới Auth Service
            4. Auth Service tạo session và lưu vào Redis
            5. Gateway nhận session_id và set HttpOnly cookie
            6. Các API calls được validate session qua Auth Service
            7. Gateway inject Bearer token vào requests tới downstream services
            
            Security Features:
            - Session-based authentication với HttpOnly cookies
            - Token management tại Auth Service (không ở Gateway)
            - Session validation middleware
            - Automatic Bearer token injection
            - RBAC/PBAC policy enforcement
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(builder.Configuration["OAuth:WebAppUrl"] ?? "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

#endregion

#region Health Checks

builder.Services.AddHealthChecks();

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

    app.MapGet("/_whoami", async (HttpContext ctx) =>
    {
        var user = ctx.User;

        // Lấy access token từ cookie auth
        var accessToken = await ctx.GetTokenAsync("access_token");
        var refreshToken = await ctx.GetTokenAsync("refresh_token");
        var idToken = await ctx.GetTokenAsync("id_token");

        return Results.Json(new
        {
            user = new
            {
                sub = user.FindFirst("sub")?.Value,
                username = user.Identity?.Name,
                realm_roles = user.FindAll("realm_access/roles").Select(c => c.Value),
                resource_roles = user.FindAll("resource_access").Select(c => c.Value),
            },
            tokens = new
            {
                access_token = accessToken,
                refresh_token = refreshToken, // Chỉ trả trong DEV
                id_token = idToken
            },
            request = new
            {
                traceId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
                correlationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            }
        });
    })
    .RequireAuthorization();
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
