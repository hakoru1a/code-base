namespace Contracts.Services
{
    public interface ISMTPEmailServices<T> : IEmailServices<T> where T : class
    {
    }
}
