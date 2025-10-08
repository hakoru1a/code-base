using Contracts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Constracts.Common.Interface
{
    public interface IRepositoryQueryBase<T, K, TContext> : IRepositoryQueryBase<T, K>
      where T : EntityBase<K>
      where TContext : DbContext
    {
    }

    public interface IRepositoryBaseAsync<T, K, TContext> : IRepositoryQueryBase<T, K, TContext>, IRepositoryBaseAsync<T, K>
        where T : EntityBase<K>
        where TContext : DbContext
    {
    }
    public interface IRepositoryBaseAsync<T, K> : IRepositoryQueryBase<T, K> where T : EntityBase<K>
    {
        Task<K> CreateAsync(T entity);
        void Create(T entity);
        Task<IList<K>> CreateListAsync(IEnumerable<T> entities);
        void Update(T entity);
        Task UpdateAsync(T entity);
        Task UpdateListAsync(IEnumerable<T> entities);
        void Delete(T entity);
        Task DeleteAsync(T entity);
        Task DeleteListAsync(IEnumerable<T> entities);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task EndTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }

    public interface IRepositoryQueryBase<T, K> where T : EntityBase<K>
    {
        IQueryable<T> FindAll(bool trackChanges = false);
        IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false,
            params Expression<Func<T, object>>[] includeProperties);
        Task<T?> GetByIdAsync(K id);
        Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties);
    }
}
