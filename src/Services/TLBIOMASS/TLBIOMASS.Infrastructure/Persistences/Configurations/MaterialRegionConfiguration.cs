using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.MaterialRegions;
using TLBIOMASS.Domain.MaterialRegions.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class MaterialRegionConfiguration : IEntityTypeConfiguration<MaterialRegion>
{
    public void Configure(EntityTypeBuilder<MaterialRegion> builder)
    {
        builder.ToTable("materialregion");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("RegionID");

        builder.OwnsOne(x => x.Detail, d =>
        {
            d.Property(x => x.RegionName).IsRequired().HasMaxLength(150).HasColumnName("RegionName");
            d.Property(x => x.Address).HasMaxLength(255).HasColumnName("Address");
            d.Property(x => x.Latitude).HasColumnName("Latitude");
            d.Property(x => x.Longitude).HasColumnName("Longitude");
            d.Property(x => x.AreaHa).HasColumnType("decimal(10,2)").HasColumnName("AreaHa");
            d.Property(x => x.CertificateId).HasMaxLength(100).HasColumnName("CertificateID");
        });

        builder.Property(x => x.OwnerId)
            .IsRequired()
            .HasColumnName("OwnerID");

        builder.HasMany(x => x.RegionMaterials)
            .WithOne(x => x.MaterialRegion)
            .HasForeignKey(x => x.MaterialRegionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
