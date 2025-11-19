using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho CORS (Cross-Origin Resource Sharing)
    /// 
    /// MỤC ĐÍCH:
    /// - Cho phép frontend (React, Angular, Vue) từ domain khác gọi API
    /// - Bảo mật: Kiểm soát origins nào được phép truy cập API
    /// - Development vs Production: Dev cho phép tất cả, Production chỉ cho phép whitelist
    /// 
    /// SỬ DỤNG:
    /// 1. Development (allow tất cả):
    ///    services.AddCorsForDevelopment();
    ///    app.UseCorsConfiguration("DevelopmentCorsPolicy");
    /// 
    /// 2. Production (chỉ cho phép specific domains):
    ///    services.AddCorsForProduction(new[] {
    ///        "https://app.company.com",
    ///        "https://admin.company.com"
    ///    });
    ///    app.UseCorsConfiguration("ProductionCorsPolicy");
    /// 
    /// LƯU Ý QUAN TRỌNG:
    /// - Phải đặt app.UseCors() TRƯỚC app.UseAuthentication() và app.UseAuthorization()
    /// - AllowCredentials() yêu cầu specify rõ origins, không dùng AllowAnyOrigin()
    /// 
    /// IMPACT:
    /// + Security: Kiểm soát ai có thể gọi API từ browser
    /// + Development: Dev có thể test frontend từ localhost dễ dàng
    /// - Security Risk: AllowAnyOrigin() trong Production = lỗ hổng bảo mật nghiêm trọng
    /// - CSRF: Cần thêm anti-CSRF token nếu AllowCredentials() = true
    /// </summary>
    public static class CorsExtensions
    {
        /// <summary>
        /// CORS configuration linh hoạt - Có thể custom hoàn toàn
        /// 
        /// CÁCH DÙNG:
        /// services.AddCorsConfiguration("MyPolicy", options => {
        ///     options.AddPolicy("MyPolicy", builder => {
        ///         builder.WithOrigins("https://trusted-site.com")
        ///                .AllowAnyMethod()
        ///                .AllowAnyHeader();
        ///     });
        /// });
        /// 
        /// HOẶC dùng default (KHÔNG AN TOÀN CHO PRODUCTION):
        /// services.AddCorsConfiguration(); // Allow tất cả origins
        /// </summary>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, string policyName = "AllowAllOrigins", Action<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>? configureOptions = null)
        {
            services.AddCors(options =>
            {
                if (configureOptions != null)
                {
                    configureOptions(options);
                }
                else
                {
                    // Default policy - Allow all origins (for development)
                    options.AddPolicy(policyName, builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
                }
            });

            return services;
        }

        /// <summary>
        /// CORS cho Production - CHỈ CHO PHÉP WHITELIST DOMAINS
        /// 
        /// CÁCH DÙNG:
        /// services.AddCorsForProduction(new[] {
        ///     "https://app.company.com",
        ///     "https://admin.company.com",
        ///     "https://mobile-api.company.com"
        /// }, "ProductionCorsPolicy");
        /// 
        /// BẢO MẬT:
        /// - AllowCredentials(): Cho phép gửi cookies/authentication headers
        /// - WithOrigins(): CHỈ whitelist domains được phép
        /// - AllowAnyMethod/Header(): Cho phép tất cả HTTP methods và headers
        /// 
        /// LƯU Ý: Không bao giờ dùng AllowAnyOrigin() trong Production!
        /// </summary>
        public static IServiceCollection AddCorsForProduction(this IServiceCollection services, string[] allowedOrigins, string policyName = "ProductionCorsPolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// CORS cho Development - CHO PHÉP TẤT CẢ ORIGINS
        /// 
        /// CÁCH DÙNG:
        /// services.AddCorsForDevelopment();
        /// 
        /// FEATURES:
        /// - SetIsOriginAllowed(_ => true): Allow mọi origin (localhost:3000, localhost:8080...)
        /// - AllowCredentials(): Cho phép gửi cookies và auth headers
        /// - AllowAnyMethod/Header(): Cho phép tất cả methods và headers
        /// 
        /// ⚠️ CẢNH BÁO: CHỈ DÙNG CHO DEVELOPMENT!
        /// Không bao giờ deploy config này lên Production - lỗ hổng bảo mật nghiêm trọng!
        /// </summary>
        public static IServiceCollection AddCorsForDevelopment(this IServiceCollection services, string policyName = "DevelopmentCorsPolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.SetIsOriginAllowed(_ => true) // Allow any origin
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Bật CORS middleware trong application pipeline
        /// 
        /// CÁCH DÙNG:
        /// app.UseCorsConfiguration("ProductionCorsPolicy");
        /// 
        /// ⚠️ QUAN TRỌNG - THỨ TỰ MIDDLEWARE:
        /// app.UseRouting();
        /// app.UseCors("PolicyName");        ← PHẢI Ở ĐÂY
        /// app.UseAuthentication();
        /// app.UseAuthorization();
        /// app.UseEndpoints(...);
        /// 
        /// Sai thứ tự = CORS không hoạt động hoặc bypass security!
        /// </summary>
        public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app, string policyName = "AllowAllOrigins")
        {
            app.UseCors(policyName);
            return app;
        }
    }
}