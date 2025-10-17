using Microsoft.EntityFrameworkCore;

namespace Contracts.Common.Interface
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        Task<int> CommitAsync();
    }
}
