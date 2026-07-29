using AiKocStudio.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
    IJwtTokenService jwtTokenService,
    ICurrentUserService currentUserService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);

        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (token is null || token.RevokedAt is not null || token.UserId != currentUserService.UserId)
        {
            return;
        }

        token.RevokedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
