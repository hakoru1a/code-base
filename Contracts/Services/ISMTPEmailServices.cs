using Shared.Services.Email;

namespace Constracts.Services
{
    public interface ISMTPEmailServices : IEmailServices<MailRequest>
    {
    }
}
