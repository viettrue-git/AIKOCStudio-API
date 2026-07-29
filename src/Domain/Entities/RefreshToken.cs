using AiKocStudio.Domain.Common;

namespace AiKocStudio.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    // Concurrency token is Postgres's own `xmin` system column, wired via
    // RefreshTokenConfiguration.UseXminAsConcurrencyToken() — EF Core tracks it
    // as a shadow property, so no property belongs here at all. Two concurrent
    // rotation attempts on the same token both load it un-revoked, but only the
    // first SaveChangesAsync wins — the second throws DbUpdateConcurrencyException,
    // which RefreshTokenCommandHandler turns into a 401 rather than silently
    // issuing two valid token pairs.

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
