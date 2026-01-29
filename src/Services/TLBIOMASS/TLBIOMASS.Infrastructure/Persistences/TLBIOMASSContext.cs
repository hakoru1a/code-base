using Contracts.Common.Events;
using Contracts.Common.Interface;
using Contracts.Domain.Interface;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces.Event;
using System.Reflection;
using TLBIOMASS.Domain.Agencies;
using TLBIOMASS.Domain.Customers;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Payments;
using TLBIOMASS.Domain.Receivers;
using TLBIOMASS.Domain.WeighingTicketCancels;
using TLBIOMASS.Domain.WeighingTickets;
using TLBIOMASS.Domain.BankAccounts;

namespace TLBIOMASS.Infrastructure.Persistences
{
    public class TLBIOMASSContext : DbContext
    {
        private readonly IMediator _mediator;

        private List<BaseEvent>? _events;

        public TLBIOMASSContext(DbContextOptions<TLBIOMASSContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Landowner> Landowners { get; set; }
        public DbSet<MaterialRegion> MaterialRegions { get; set; }
        public DbSet<RegionMaterial> RegionMaterials { get; set; }
        public DbSet<Receiver> Receivers { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<WeighingTicket> WeighingTickets { get; set; }
        public DbSet<WeighingTicketCancel> WeighingTicketCancels { get; set; }
        public DbSet<WeighingTicketPayment> WeighingTicketPayments { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

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
                .SelectMany(x => x.DomainEvents.Cast<BaseEvent>())
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
                            addedEntity.CreatedDate = DateTimeOffset.UtcNow;
                        }
                        break;

                    case EntityState.Modified:
                        var idProperty = item.Entity.GetType().GetProperty("Id");
                        if (idProperty != null)
                        {
                            Entry(item.Entity).Property(idProperty.Name).IsModified = false;
                        }
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = DateTimeOffset.UtcNow;
                        }
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            // Domain event dispatch intentionally disabled until handlers are ready.
            return result;
        }
    }
}
