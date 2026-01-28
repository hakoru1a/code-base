using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLBIOMASS.Domain.Materials;
using TLBIOMASS.Domain.Materials.ValueObjects;

namespace TLBIOMASS.Infrastructure.Persistences.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("hanghoa");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.OwnsOne(x => x.Spec, s =>
        {
            s.Property(x => x.Name).IsRequired().HasMaxLength(200).HasColumnName("ten_hang_hoa");
            s.Property(x => x.Unit).IsRequired().HasMaxLength(50).HasColumnName("don_vi_tinh");
            s.Property(x => x.Description).HasColumnType("text").HasColumnName("mo_ta");
            s.Property(x => x.ProposedImpurityDeduction).HasColumnType("decimal(5,2)").HasColumnName("de_xuat_tru_tap_chat");
        });

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
