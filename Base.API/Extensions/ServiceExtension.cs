using Infrastructure.Configurations;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Base.API.Extensions
{
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Config();
            return services;
        }

        private static void Config(this IServiceCollection services)
        {
            var emailSettings = services.GetOptions<SMTPEmailSettings>(sectionName: nameof(SMTPEmailSettings));
            services.AddSingleton(emailSettings);

        }
    }
}
