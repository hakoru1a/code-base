using Contracts.Domain.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public static class AuditConfigurationExtensions
    {
        public static void MapAuditColumns<T>(this EntityTypeBuilder<T> builder)
            where T : class, IDateTracking, IUserTracking<long>
        {
            builder.Property(e => e.CreatedDate)
                .HasColumnName("CREATED_DATE");

            builder.Property(e => e.LastModifiedDate)
                .HasColumnName("LAST_MODIFIED_DATE");

            builder.Property(e => e.CreatedBy)
                .HasColumnName("CREATED_BY");

            builder.Property(e => e.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
        }
    }
}


