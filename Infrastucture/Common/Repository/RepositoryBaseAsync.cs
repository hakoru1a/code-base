using Contracts.Common.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Domain;
using Shared.SeedWork;

namespace Infrastucture.Common.Repository
{
    /// <summary>
    /// Base repository implementation providing common data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="TContext">DbContext type</typeparam>
    public class RepositoryBaseAsync<T, K, TContext> : IRepositoryBaseAsync<T, K, TContext>
        where T : EntityBase<K>
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly IUnitOfWork<TContext> _unitOfWork;

        public RepositoryBaseAsync(TContext dbContext, IUnitOfWork<TContext> unitOfWork)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        #region Query Methods

        /// <summary>
        /// Finds all entities with optional change tracking
        /// </summary>
        public IQueryable<T> FindAll(bool trackChanges = false) =>
            !trackChanges ? _dbContext.Set<T>().AsNoTracking() :
                _dbContext.Set<T>();

        /// <summary>
        /// Finds all entities with eager loading of related properties
        /// </summary>
        public IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindAll(trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        /// <summary>
        /// Finds entities by condition with optional change tracking
        /// </summary>
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
            !trackChanges
                ? _dbContext.Set<T>().Where(expression).AsNoTracking()
                : _dbContext.Set<T>().Where(expression);

        /// <summary>
        /// Finds entities by condition with eager loading of related properties
        /// </summary>
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = FindByCondition(expression, trackChanges);
            items = includeProperties.Aggregate(items, (current, includeProperty) => current.Include(includeProperty));
            return items;
        }

        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        public async Task<T?> GetByIdAsync(K id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return await FindByCondition(expression: x => x.Id!.Equals(id))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets an entity by its ID with eager loading of related properties
        /// </summary>
        public async Task<T?> GetByIdAsync(K id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return await FindByCondition(expression: x => x.Id!.Equals(id), trackChanges: false, includeProperties)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets paginated results from a query
        /// </summary>
        public async Task<PagedList<T>> GetPageAsync(IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<T>(items, totalItems, pageNumber, pageSize);
        }

        #endregion

        #region Transaction Methods

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => _dbContext.Database.BeginTransactionAsync(cancellationToken);

        /// <summary>
        /// Commits the current transaction with save changes
        /// </summary>
        public async Task EndTransactionAsync(CancellationToken cancellationToken = default)
        {
            await SaveChangesAsync(cancellationToken);
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => _dbContext.Database.RollbackTransactionAsync(cancellationToken);

        #endregion

        #region Create Methods

        /// <summary>
        /// Creates a new entity and saves changes immediately
        /// </summary>
        public async Task<K> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        /// <summary>
        /// Adds an entity to the context without saving (use SaveChangesAsync to persist)
        /// </summary>
        public async Task CreateWithoutSaveAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Creates multiple entities without saving immediately (use SaveChangesAsync to persist)
        /// </summary>
        public async Task<IList<K>> CreateListAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any())
                return new List<K>();

            await _dbContext.Set<T>().AddRangeAsync(entityList, cancellationToken);
            return entityList.Select(x => x.Id).ToList();
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates an entity without saving (use SaveChangesAsync to persist)
        /// </summary>
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_dbContext.Entry(entity).State == EntityState.Unchanged)
                return;

            var exist = await _dbContext.Set<T>().FindAsync(new object[] { entity.Id! }, cancellationToken);
            if (exist == null)
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");

            _dbContext.Entry(exist).CurrentValues.SetValues(entity);
        }

        /// <summary>
        /// Updates an entity and saves changes immediately
        /// </summary>
        public async Task UpdateAndSaveAsync(T entity, CancellationToken cancellationToken = default)
        {
            await UpdateAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates multiple entities without saving (use SaveChangesAsync to persist)
        /// </summary>
        public async Task UpdateListAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any())
                return;

            foreach (var entity in entityList)
            {
                await UpdateAsync(entity, cancellationToken);
            }
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Deletes an entity without saving (use SaveChangesAsync to persist)
        /// </summary>
        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbContext.Set<T>().Remove(entity);
        }

        /// <summary>
        /// Deletes an entity and saves changes immediately
        /// </summary>
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbContext.Set<T>().Remove(entity);
            await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes multiple entities without saving (use SaveChangesAsync to persist)
        /// </summary>
        public Task DeleteListAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any())
                return Task.CompletedTask;

            _dbContext.Set<T>().RemoveRange(entityList);
            return Task.CompletedTask;
        }

        #endregion

        /// <summary>
        /// Saves all changes made in the context
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _dbContext.SaveChangesAsync(cancellationToken);
    }
}
