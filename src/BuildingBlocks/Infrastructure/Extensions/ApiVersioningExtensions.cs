using Microsoft.Extensions.DependencyInjection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

namespace Infrastructure.Extensions
{
    public static class ApiVersioningExtensions
    {
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