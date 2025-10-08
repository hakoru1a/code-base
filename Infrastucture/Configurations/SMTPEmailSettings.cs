using Shared.Configurations;

namespace Infrastructure.Configurations
{
    public class SMTPEmailSettings : IEmailSMTPSettings
    {
        public string DisplayName { get; set; }
        public bool EnableVerification { get; set; }
        public string From { get; set; }
        public string SMTPServer { get; set; }
        public bool UseSSL { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
