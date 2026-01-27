using Contracts.Common.Events;
using Contracts.Common.Interface;
using Contracts.Domain.Interface;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces.Event;
using System.Reflection;

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

        // TODO: Add DbSets as features are developed
        public DbSet<TLBIOMASS.Domain.Receivers.Receiver> Receivers { get; set; }
        public DbSet<TLBIOMASS.Domain.Agencies.Agency> Agencies { get; set; }
        public DbSet<TLBIOMASS.Domain.Materials.Material> Materials { get; set; }

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
            //if (_events?.Any() == true)
            //{
            //    await _mediator.DispatchDomainEventAsync(_events);
            //}
            return result;
        }
    }
}