using Contracts.Common;
using Contracts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Contracts.Common.Interface
{
    /// <summary>
    /// Base query interface for repositories with DbContext
    /// </summary>
    public interface IRepositoryQueryBase<T, K, TContext> : IRepositoryQueryBase<T, K>
      where T : EntityBase<K>
      where TContext : DbContext
    {
    }

    /// <summary>
    /// Base async repository interface with DbContext
    /// </summary>
    public interface IRepositoryBaseAsync<T, K, TContext> : IRepositoryQueryBase<T, K, TContext>, IRepositoryBaseAsync<T, K>
        where T : EntityBase<K>
        where TContext : DbContext
    {
    }

    /// <summary>
    /// Base async repository interface for CRUD operations
    /// </summary>
    public interface IRepositoryBaseAsync<T, K> : IRepositoryQueryBase<T, K> where T : EntityBase<K>
    {
        // Create operations
        Task<K> CreateAsync(T entity, CancellationToken cancellationToken = default);
        Task CreateWithoutSaveAsync(T entity, CancellationToken cancellationToken = default);
        Task<IList<K>> CreateListAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Update operations
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAndSaveAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateListAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Delete operations
        void Delete(T entity);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteListAsync(IEnumerable<T> entities);

        // Transaction operations
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task EndTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        // Save changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Bulk operations (EF Core 7+)
        Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkInsertAsync(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteAsync(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteByConditionAsync(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateByConditionAsync(Expression<Func<T, bool>> condition, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls, CancellationToken cancellationToken = default);
        Task<int> BulkUpsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpsertAsync(IEnumerable<T> entities, int batchSize = 1000, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Base query interface for reading operations
    /// </summary>
    public interface IRepositoryQueryBase<T, K> where T : EntityBase<K>
    {
        IQueryable<T> FindAll(bool trackChanges = false);
        IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false,
            params Expression<Func<T, object>>[] includeProperties);
        Task<T?> GetByIdAsync(K id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(K id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties);
        Task<PagedList<T>> GetPageAsync(IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
