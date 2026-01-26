using Contracts.Common.Events;
using Contracts.Common.Interface;
using Contracts.Domain.Interface;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces.Event;
using System.Reflection;
using TLBIOMASS.Domain.Customers;

namespace TLBIOMASS.Infrastructure.Persistences
{
    public class TLBIOMASSContext : DbContext
    {
        private IMediator _mediator;

        private List<BaseEvent>? _events;

        public TLBIOMASSContext(DbContextOptions<TLBIOMASSContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        // DbSets
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<BaseEvent>();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        private void SaveEventBeforeSaveChanges()
        {
            var domainEntities = ChangeTracker.Entries<IEventEntity>()
                .Select(x => x.Entity)
                .Where(x => x.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            _events = domainEvents;
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SaveEventBeforeSaveChanges();

            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                           e.State == EntityState.Added ||
                           e.State == EntityState.Deleted);

            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = DateTime.UtcNow;
                        }
                        else if (item.Entity is Customer customerAdded)
                        {
                            customerAdded.CreatedAt = DateTime.Now;
                        }
                        break;

                    case EntityState.Modified:
                        if (item.Entity is not Customer)
                        {
                            var idProperty = item.Entity.GetType().GetProperty("Id");
                            if (idProperty != null)
                            {
                                Entry(item.Entity).Property(idProperty.Name).IsModified = false;
                            }
                        }

                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                        }
                        else if (item.Entity is Customer customerModified)
                        {
                            customerModified.UpdatedAt = DateTime.Now;
                        }
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            //if (_events?.Any() == true)
            //{
            //    await _mediator.DispatchDomainEventAsync(_events);
            //}
            return result;
        }
    }
}