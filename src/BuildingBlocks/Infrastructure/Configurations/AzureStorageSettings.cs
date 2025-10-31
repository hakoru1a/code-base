namespace Infrastructure.Configurations
{
    public class AzureStorageSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string? AccountName { get; set; }
        public string? AccountKey { get; set; }
    }
}


