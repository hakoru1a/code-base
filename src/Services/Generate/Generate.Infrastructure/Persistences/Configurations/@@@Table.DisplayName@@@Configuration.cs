@@@copyRight@@@
using Generate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Generate.Infrastructure.Persistences.Configurations
{
    public class @@@Table.DisplayName @@@Configuration : IEntityTypeConfiguration<@@@Table.DisplayName@@@>
    {
        public void Configure(EntityTypeBuilder<@@@Table.DisplayName@@@> builder)
    {
        // Table name
        builder.ToTable("@@@LoopTable.Name@@@");

        // Primary key
        builder.HasKey(c => c.Id);

            // Properties
< FOREACH >
    < LOOP > TableGenerate.ColumnsNotKey </ LOOP >
    < CONTENT > builder.Property(c => c.###ColumnName###)
                .IsRequired(###DbNotNull###)
                .HasMaxLength(###DbLength###);</CONTENT>
</ FOREACH >

            // Relationships
< FOREACH >
    < LOOP > Tables.ForeignTable </ LOOP >
    < CONTENT > builder.HasOne(c => c.@@@TableForeign.Name@@@)
                .WithMany(p => p.@@@Table.DisplayName@@@s)
                .HasForeignKey(p => p.@@@TableForeign.Name@@@Id)
                .OnDelete(DeleteBehavior.SetNull);</ CONTENT >
</ FOREACH >

< FOREACH >
    < LOOP > Tables.ForeignKeyBy </ LOOP >
    < CONTENT > builder.HasMany(c => c.@@@TableForeignBy.Name@@@s)
                .WithOne(p => p.@@@Table.DisplayName@@@)
                .HasForeignKey(p => p.@@@Table.DisplayName@@@Id)
                .OnDelete(DeleteBehavior.SetNull);</ CONTENT >
</ FOREACH >

            // Indexes
< FOREACH >
    < LOOP > TableGenerate.ColumnsNotKey </ LOOP >
    < CONTENT > builder.HasIndex(c => c.###ColumnName###);</CONTENT>
</ FOREACH >

            builder.MapAuditColumns();
    }
}
}