namespace ApiGateway.Configurations;

/// <summary>
/// Options for individual service configuration
/// </summary>
public class ServiceOptions
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IncludeInSwagger { get; set; }
    public bool IncludeInHealthChecks { get; set; }
    public string HealthCheckPath { get; set; } = "/health";
}

/// <summary>
/// All services configuration
/// </summary>
public class ServicesOptions
{
    public const string SectionName = "Services";

    public ServiceOptions AuthAPI { get; set; } = new();
    public ServiceOptions BaseAPI { get; set; } = new();
    public ServiceOptions GenerateAPI { get; set; } = new();
}

