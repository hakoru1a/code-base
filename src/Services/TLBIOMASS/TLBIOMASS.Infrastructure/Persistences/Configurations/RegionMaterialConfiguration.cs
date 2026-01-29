using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.MaterialRegions;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class RegionMaterialConfiguration : IEntityTypeConfiguration<RegionMaterial>
{
    public void Configure(EntityTypeBuilder<RegionMaterial> builder)
    {
        builder.ToTable("material_region_materials");
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .HasColumnName("Status");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");

        builder.Property(x => x.MaterialRegionId)
            .IsRequired()
            .HasColumnName("RegionID");

        builder.Property(x => x.MaterialId)
            .IsRequired()
            .HasColumnName("MaterialID");

        builder.Property(x => x.AreaHa)
            .HasColumnType("decimal(10,2)")
            .HasColumnName("AreaHa");

        // Audit mappings
        builder.Property(x => x.CreatedDate).HasColumnName("Created_At");
        builder.Property(x => x.CreatedBy).HasColumnName("Created_By");
        builder.Property(x => x.LastModifiedDate).HasColumnName("Last_Updated_At");
        builder.Property(x => x.LastModifiedBy).HasColumnName("Last_Updated_By");
    }
}
