using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.Common.Models;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (user is null || !user.IsActive || !identityService.VerifyPassword(user, request.Password))
        {
            throw new AuthenticationFailedException();
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = jwtTokenService.HashRefreshToken(rawRefreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.Add(AuthConstants.RefreshTokenLifetime),
        });

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResult(accessToken, rawRefreshToken);
    }
}
