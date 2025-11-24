using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Configurations
{
    public interface IEmailSMTPSettings
    {
        string? DisplayName { get; set; }
        bool EnableVerification { get; set; }
        string? From { get; set; }
        string? SMTPServer { get; set; }
        bool UseSSL { get; set; }
        int Port { get; set; }
        string? Username { get; set; }
        string? Password { get; set; }
    }
}
