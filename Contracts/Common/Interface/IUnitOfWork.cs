using Microsoft.EntityFrameworkCore;

namespace Constracts.Common.Interface
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        Task<int> CommitAsync();
    }
}
