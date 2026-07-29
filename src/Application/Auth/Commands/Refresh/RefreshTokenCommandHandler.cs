using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.Common.Models;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = jwtTokenService.HashRefreshToken(request.RefreshToken);

        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, cancellationToken);

        if (token is null)
        {
            throw new AuthenticationFailedException();
        }

        if (token.RevokedAt is not null)
        {
            // Reuse of an already-rotated/revoked token is a breach signal:
            // revoke every active token for this user to kill all sessions.
            var activeTokens = await context.RefreshTokens
                .Where(t => t.UserId == token.UserId && t.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokedAt = DateTimeOffset.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
            throw new AuthenticationFailedException();
        }

        if (token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new AuthenticationFailedException();
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == token.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new AuthenticationFailedException();
        }

        var newRawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newHash = jwtTokenService.HashRefreshToken(newRawRefreshToken);

        token.RevokedAt = DateTimeOffset.UtcNow;
        token.ReplacedByTokenHash = newHash;

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = DateTimeOffset.UtcNow.Add(AuthConstants.RefreshTokenLifetime),
        });

        var newAccessToken = jwtTokenService.GenerateAccessToken(user);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another concurrent refresh call rotated this exact token first
            // (Postgres's xmin system column, used as the concurrency token —
            // see RefreshTokenConfiguration — changed under us) — treat it the
            // same as an invalid token rather than silently issuing a second pair.
            throw new AuthenticationFailedException();
        }

        return new AuthResult(newAccessToken, newRawRefreshToken);
    }
}
