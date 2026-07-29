using AiKocStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiKocStudio.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Category).HasMaxLength(200);

        builder.HasOne<Persona>()
            .WithMany()
            .HasForeignKey(p => p.TargetPersonaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
