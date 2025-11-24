using Contracts.Services;
using Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;
using Shared.Configurations;
using Shared.Services.Email;
namespace Infrastructure.Services
{
    public class SMTTEmailServices : ISMTPEmailServices
    {
        private readonly ILogger _logger;

        private readonly SMTPEmailSettings _emailSMTPSettings;

        private readonly SmtpClient _smtpClient;

        public SMTTEmailServices(ILogger logger, SMTPEmailSettings emailSMTPSettings)
        {
            _logger = logger;
            _emailSMTPSettings = emailSMTPSettings;
            _smtpClient = new SmtpClient();
        }
        public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            //var emailMessage = new MimeMessage
            //{
            //    Sender = new MailboxAddress(_emailSMTPSettings.DisplayName, address: request.From ?? _emailSMTPSettings.From),
            //    Subject = request.Subject,
            //    Body = new BodyBuilder
            //    {
            //        HtmlBody = request.Body
            //    }.ToMessageBody() // MimeEntity
            //};

            //if (request.ToAddresses.Any())
            //{
            //    foreach (var toAddress in request.ToAddresses)
            //    {
            //        emailMessage.To.Add(MailboxAddress.Parse(toAddress));
            //    }
            //}
            //else
            //{
            //    var toAddress = request.ToAddress;
            //    emailMessage.To.Add(MailboxAddress.Parse(toAddress));
            //}

            await Task.CompletedTask;
            _logger.Information("send mail success....");
        }
    }
}
