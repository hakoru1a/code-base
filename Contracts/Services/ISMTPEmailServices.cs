using Shared.Services.Email;

namespace Contracts.Services
{
    public interface ISMTPEmailServices : IEmailServices<MailRequest>
    {
    }
}
