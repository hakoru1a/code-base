namespace Infrastructure.Configurations
{
    public class FcmSettings
    {
        public string ProjectId { get; set; } = string.Empty;
        public string CredentialPath { get; set; } = string.Empty;
        public string ServerKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string FcmUrl { get; set; } = "https://fcm.googleapis.com/v1/projects";
    }
}


