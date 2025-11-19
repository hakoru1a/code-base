using Microsoft.Extensions.DependencyInjection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho API Versioning - Quản lý multiple versions của API
    /// 
    /// MỤC ĐÍCH:
    /// - Hỗ trợ nhiều phiên bản API chạy đồng thời (v1, v2, v3...)
    /// - Cho phép deprecated version cũ mà không break client đang dùng
    /// - Cung cấp nhiều cách specify version: header, query string, URL segment
    /// 
    /// SỬ DỤNG:
    /// 1. Đăng ký API versioning:
    ///    services.AddApiVersioningConfiguration(majorVersion: 1, minorVersion: 0);
    /// 
    /// 2. Đánh dấu version trên Controller:
    ///    [ApiVersion("1.0")]
    ///    [ApiVersion("2.0")]
    ///    public class ProductsController : ControllerBase { }
    /// 
    /// 3. Client gọi API với version:
    ///    - Header: x-api-version: 1.0
    ///    - Query: /api/products?version=1.0
    ///    - URL: /api/v1/products
    /// 
    /// IMPACT:
    /// + Backward Compatibility: Có thể maintain nhiều versions, không break old clients
    /// + Gradual Migration: Client migrate từ v1 sang v2 theo tốc độ riêng
    /// + Clear Contract: Mỗi version có contract rõ ràng, dễ document
    /// - Maintenance Cost: Phải maintain code cho nhiều versions cùng lúc
    /// - Testing Overhead: Cần test tất cả versions đang active
    /// </summary>
    public static class ApiVersioningExtensions
    {
        /// <summary>
        /// Cấu hình API versioning với multiple readers (header + query + url)
        /// 
        /// CÁCH DÙNG:
        /// services.AddApiVersioningConfiguration(
        ///     majorVersion: 1,
        ///     minorVersion: 0,
        ///     headerName: "x-api-version",  // Client gửi: x-api-version: 1.0
        ///     groupNameFormat: "'v'VVV"      // Hiển thị trong Swagger: v1, v2
        /// );
        /// 
        /// SUPPORT NHIỀU CÁCH GỬI VERSION:
        /// - Header: x-api-version: 1.0
        /// - Query: ?version=1.0
        /// - URL: /api/v1/products
        /// </summary>
        public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services,
            int majorVersion = 1,
            int minorVersion = 0,
            string headerName = "x-api-version",
            string groupNameFormat = "'v'VVV")
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Configure how API version is read from requests
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader(headerName),
                    new QueryStringApiVersionReader("version"),
                    new UrlSegmentApiVersionReader()
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = groupNameFormat;
                options.SubstituteApiVersionInUrl = false;
            });

            return services;
        }

        /// <summary>
        /// API versioning qua Query String - Đơn giản, dễ test trên browser
        /// 
        /// CÁCH DÙNG: GET /api/products?version=1.0
        /// PHÙ HỢP: Public APIs, APIs dễ test trên browser
        /// </summary>
        public static IServiceCollection AddApiVersioningWithQueryString(this IServiceCollection services,
            int majorVersion = 1,
            int minorVersion = 0,
            string queryParameterName = "version")
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new QueryStringApiVersionReader(queryParameterName);
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });

            return services;
        }

        /// <summary>
        /// API versioning qua URL segment - RESTful style
        /// 
        /// CÁCH DÙNG: GET /api/v1/products, GET /api/v2/products
        /// PHÙ HỢP: RESTful APIs, dễ routing, clear separation giữa versions
        /// LƯU Ý: Controller route phải có {version:apiVersion}: [Route("api/v{version:apiVersion}/[controller]")]
        /// </summary>
        public static IServiceCollection AddApiVersioningWithUrlSegment(this IServiceCollection services,
            int majorVersion = 1,
            int minorVersion = 0)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        /// <summary>
        /// API versioning với nhiều cách đọc version (linh hoạt nhất)
        /// 
        /// SUPPORT:
        /// - Header: x-api-version: 1.0
        /// - Query String: ?version=1.0
        /// - URL Segment: /api/v1/products
        /// - Media Type: Accept: application/json;ver=1.0
        /// 
        /// PHÙ HỢP: Enterprise APIs cần hỗ trợ nhiều loại clients
        /// </summary>
        public static IServiceCollection AddApiVersioningWithMultipleReaders(this IServiceCollection services,
            int majorVersion = 1,
            int minorVersion = 0,
            string headerName = "x-api-version",
            string queryParameterName = "version")
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Support multiple ways to specify API version
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader(headerName),
                    new QueryStringApiVersionReader(queryParameterName),
                    new UrlSegmentApiVersionReader(),
                    new MediaTypeApiVersionReader("ver")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });

            return services;
        }
    }
}