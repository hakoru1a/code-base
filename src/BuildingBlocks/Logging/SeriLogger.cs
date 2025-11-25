using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
namespace Common.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
          (context, configuration) =>
          {
              var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
              var environmentName = context.HostingEnvironment.EnvironmentName ?? "Development";

              var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
              var elasticUsername = context.Configuration.GetValue<string>("ElasticConfiguration:Username");
              var elasticPassword = context.Configuration.GetValue<string>("ElasticConfiguration:Password");

              // Base configuration - Console and File logging
              var baseConfig = configuration
                  .WriteTo.Console(outputTemplate:
                      "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}")
                  .WriteTo.File("logs/log-.txt",
                      rollingInterval: RollingInterval.Day,
                      outputTemplate:
                      "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}");

              // Only enable Debug sink in Development environment
#if DEBUG
              baseConfig = baseConfig.WriteTo.Debug();
#endif

              if (!string.IsNullOrEmpty(elasticUri))
              {
                  baseConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                  {
                      IndexFormat = $"ch-logs-{applicationName}-{environmentName}-{DateTime.UtcNow:yyyy-MM}",
                      AutoRegisterTemplate = true,
                      AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                      DetectElasticsearchVersion = true,
                      NumberOfShards = 2,
                      NumberOfReplicas = 1,
                      ModifyConnectionSettings = x => x.BasicAuthentication(elasticUsername, elasticPassword),
                  });
              }

              configuration
                  .Enrich.FromLogContext()
                  .Enrich.WithMachineName()
                  .Enrich.WithProperty("Environment", environmentName)
                  .Enrich.WithProperty("Application", applicationName)
                  .ReadFrom.Configuration(context.Configuration);
          };
    }
}
