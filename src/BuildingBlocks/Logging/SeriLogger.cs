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
              var elasicUsername = context.Configuration.GetValue<string>("ElasticConfiguration:Username");
              var elasicPassword = context.Configuration.GetValue<string>("ElasticConfiguration:Password");

              if (!string.IsNullOrEmpty(elasticUri))
              {
                  configuration
                      .WriteTo.Debug()
                      .WriteTo.Console(outputTemplate:
                          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}")
                      .WriteTo.File("logs/log-.txt",
                          rollingInterval: RollingInterval.Day,
                          outputTemplate:
                          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}")
                      .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                      {
                          IndexFormat = $"ch-logs-{applicationName}-{environmentName}-{DateTime.UtcNow:yyyy-MM-dd}",
                          AutoRegisterTemplate = true,
                          AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                          DetectElasticsearchVersion = true,
                          NumberOfShards = 2,
                          NumberOfReplicas = 1,
                          ModifyConnectionSettings = x => x.BasicAuthentication(elasicUsername, elasicPassword),
                      });
              }
              else
              {
                  configuration
                      .WriteTo.Debug()
                      .WriteTo.Console(outputTemplate:
                          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}")
                      .WriteTo.File("logs/log-.txt",
                          rollingInterval: RollingInterval.Day,
                          outputTemplate:
                          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:l}{NewLine}{Exception}{NewLine}");
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
