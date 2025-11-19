using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Infrastructure.Filters;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho Swagger/OpenAPI documentation
    /// 
    /// MỤC ĐÍCH:
    /// - Tự động generate API documentation từ code
    /// - Cung cấp Swagger UI để test API trực tiếp trên browser
    /// - Hỗ trợ API versioning và JWT authentication trong Swagger UI
    /// 
    /// SỬ DỤNG:
    /// 1. Trong Program.cs/Startup.cs:
    ///    services.AddSwaggerConfiguration("Product API", "API quản lý sản phẩm", "Dev Team", "dev@company.com");
    ///    app.UseSwaggerConfiguration(); // hoặc app.UseSwaggerConfiguration("api-docs") để đổi route
    /// 
    /// 2. Truy cập Swagger UI tại: https://localhost:5001/swagger
    /// 
    /// IMPACT:
    /// + Developer Experience: Dev có thể test API ngay trên browser không cần Postman
    /// + Documentation: API docs luôn sync với code, không bị outdated
    /// + Client Integration: Frontend/Mobile team có thể generate client code từ OpenAPI spec
    /// + JWT Testing: Có thể nhập JWT token để test các endpoint có authentication
    /// - Performance: Chỉ nên enable trong Development/Staging, không nên expose trong Production
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Đăng ký Swagger với cấu hình đầy đủ (API versioning + JWT authentication)
        /// 
        /// CÁCH DÙNG:
        /// services.AddSwaggerConfiguration(
        ///     apiTitle: "Order Service API",
        ///     apiDescription: "API quản lý đơn hàng và thanh toán",
        ///     contactName: "Backend Team",
        ///     contactEmail: "backend@company.com"
        /// );
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, string apiTitle = "API", string apiDescription = "A Web API service", string contactName = "API Team", string contactEmail = "support@api.com")
        {
            // Register the Swagger configuration options
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(provider => 
                new ConfigureSwaggerOptions(
                    provider.GetRequiredService<IApiVersionDescriptionProvider>(),
                    apiTitle,
                    apiDescription,
                    contactName,
                    contactEmail));
            
            services.AddSwaggerGen(options =>
            {
                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Add operation filter for API versioning
                options.OperationFilter<ApiVersionOperationFilter>();

                // Enable XML comments if available
                var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        /// <summary>
        /// Bật Swagger UI trong application pipeline
        /// 
        /// CÁCH DÙNG:
        /// app.UseSwaggerConfiguration(); // Mặc định tại /swagger
        /// app.UseSwaggerConfiguration("api-docs"); // Custom route: /api-docs
        /// 
        /// LƯU Ý: Nên đặt sau UseRouting() và trước UseEndpoints()
        /// </summary>
        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, string routePrefix = "swagger")
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = routePrefix;
            });

            return app;
        }

        // Configure Swagger options using IConfigureOptions pattern
        public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;
            private readonly string _apiTitle;
            private readonly string _apiDescription;
            private readonly string _contactName;
            private readonly string _contactEmail;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, string apiTitle, string apiDescription, string contactName, string contactEmail)
            {
                _provider = provider;
                _apiTitle = apiTitle;
                _apiDescription = apiDescription;
                _contactName = contactName;
                _contactEmail = contactEmail;
            }

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = _apiTitle,
                        Description = _apiDescription,
                        Contact = new OpenApiContact
                        {
                            Name = _contactName,
                            Email = _contactEmail
                        }
                    });
                }
            }
        }
    }
}