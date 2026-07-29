using AiKocStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiKocStudio.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);

        builder.Property(t => t.RowVersion).IsRowVersion();

        // RefreshToken has no ITenantScoped/TenantId of its own — it's always
        // reached via its owning User (see UserConfiguration's HasMany), whose
        // TenantId already scopes access.
        builder.Ignore(t => t.IsActive);
    }
}
