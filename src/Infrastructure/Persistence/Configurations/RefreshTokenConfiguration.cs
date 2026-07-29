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

        // Postgres has no native auto-updating rowversion/timestamp column like SQL
        // Server — a byte[] property with .IsRowVersion() expects the app to supply
        // a value and fails a NOT NULL insert. `xmin` is Postgres's own system column
        // that already changes on every row update; this wires it as a shadow-property
        // concurrency token with no real column created.
#pragma warning disable CS0618 // no non-obsolete equivalent exists yet for this pattern
        builder.UseXminAsConcurrencyToken();
#pragma warning restore CS0618

        // RefreshToken has no ITenantScoped/TenantId of its own — it's always
        // reached via its owning User (see UserConfiguration's HasMany), whose
        // TenantId already scopes access.
        builder.Ignore(t => t.IsActive);
    }
}
