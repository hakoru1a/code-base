namespace Shared.Configurations;

public class FileStorageSettings
{
    public AWSStorageSettings? AWS { get; set; }
    public AzureStorageSettings? Azure { get; set; }
    public LocalStorageSettings? Local { get; set; }
}

public class AWSStorageSettings
{
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? BucketName { get; set; }
    public string? Region { get; set; }
}

public class AzureStorageSettings
{
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
}

public class LocalStorageSettings
{
    public string? BasePath { get; set; }
    public string? BaseUrl { get; set; }
}

