using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.MaterialRegions;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class MaterialRegionConfiguration : IEntityTypeConfiguration<MaterialRegion>
{
    public void Configure(EntityTypeBuilder<MaterialRegion> builder)
    {
        builder.ToTable("materialregion");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("RegionID");

        builder.Property(x => x.RegionName)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("RegionName");

        builder.Property(x => x.Address)
            .HasMaxLength(255)
            .HasColumnName("Address");

        builder.Property(x => x.Latitude)
            .HasColumnName("Latitude");

        builder.Property(x => x.Longitude)
            .HasColumnName("Longitude");

        builder.Property(x => x.AreaHa)
            .HasColumnType("decimal(10,2)")
            .HasColumnName("AreaHa");

        builder.Property(x => x.CertificateID)
            .HasMaxLength(100)
            .HasColumnName("CertificateID");

        builder.Property(x => x.OwnerId)
            .IsRequired()
            .HasColumnName("OwnerID");

        // Relationships
        builder.HasMany(x => x.RegionMaterials)
            .WithOne(x => x.MaterialRegion)
            .HasForeignKey(x => x.MaterialRegionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
