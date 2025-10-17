using Contracts.Common.Events;
using Contracts.Common.Interface;
using Contracts.Domain.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Persistence
{
    public class BaseContext : DbContext
    {
        private IMediator _mediator;

        // private readonly IPublishEndpoint _publishEndpoint;    
        // private readonly ISendEndpointProvider _sendEndpointProvider; 

        private List<BaseEvent>? _events;

        public BaseContext(DbContextOptions<BaseContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

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
                            item.State = EntityState.Added;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = DateTime.UtcNow;
                            item.State = EntityState.Modified;
                        }
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            if (_events?.Any() == true)
            {
                await _mediator.DispatchDomainEventAsync(_events);
            }
            return result;
        }
    }
}
