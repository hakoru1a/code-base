    namespace Generate.API.Extensions
{
    public static class HostExtension
    {
        public static void AddConfiguration(this ConfigureHostBuilder host)
        {
            host.ConfigureAppConfiguration(configureDelegate: (context, config) =>
            {
                IHostEnvironment env = context.HostingEnvironment;
                config.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables();
            });
        }
    }
}

