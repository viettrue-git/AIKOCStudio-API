using AiKocStudio.Application.Auth.Commands.Refresh;
using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.UnitTests.Common;
using AiKocStudio.Domain.Entities;
using AiKocStudio.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AiKocStudio.Application.UnitTests.Auth;

public class RefreshTokenCommandTests
{
    private static User SeedUser(TestApplicationDbContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed",
            DisplayName = "Test User",
            Role = UserRole.Member,
            IsActive = true,
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Mock<IJwtTokenService> MockJwtTokenService(string newRawToken = "new-raw-token", string newHash = "new-hash")
    {
        var mock = new Mock<IJwtTokenService>();
        mock.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
        mock.Setup(s => s.GenerateRefreshToken()).Returns(newRawToken);
        mock.Setup(s => s.HashRefreshToken(newRawToken)).Returns(newHash);
        return mock;
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesAndReturnsNewTokens()
    {
        using var context = TestApplicationDbContext.Create();
        var user = SeedUser(context);

        var oldToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };
        context.RefreshTokens.Add(oldToken);
        context.SaveChanges();

        var jwtTokenService = MockJwtTokenService();
        jwtTokenService.Setup(s => s.HashRefreshToken("old-raw-token")).Returns("old-hash");

        var handler = new RefreshTokenCommandHandler(context, jwtTokenService.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old-raw-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-raw-token");

        var refreshedOldToken = await context.RefreshTokens.SingleAsync(t => t.Id == oldToken.Id);
        refreshedOldToken.RevokedAt.Should().NotBeNull();
        refreshedOldToken.ReplacedByTokenHash.Should().Be("new-hash");

        var newToken = await context.RefreshTokens.SingleAsync(t => t.TokenHash == "new-hash");
        newToken.UserId.Should().Be(user.Id);
        newToken.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReusedRevokedToken_ThrowsAndRevokesAllActiveTokensForUser()
    {
        using var context = TestApplicationDbContext.Create();
        var user = SeedUser(context);

        var revokedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "revoked-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            RevokedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        };
        var otherActiveToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "other-active-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };
        context.RefreshTokens.AddRange(revokedToken, otherActiveToken);
        context.SaveChanges();

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(s => s.HashRefreshToken("reused-raw-token")).Returns("revoked-hash");

        var handler = new RefreshTokenCommandHandler(context, jwtTokenService.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("reused-raw-token"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();

        var refreshedOtherToken = await context.RefreshTokens.SingleAsync(t => t.Id == otherActiveToken.Id);
        refreshedOtherToken.RevokedAt.Should().NotBeNull("reusing a revoked token is a breach signal that should kill all of the user's sessions");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsAuthenticationFailedException()
    {
        using var context = TestApplicationDbContext.Create();
        var user = SeedUser(context);

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "expired-hash",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
        });
        context.SaveChanges();

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(s => s.HashRefreshToken("expired-raw-token")).Returns("expired-hash");

        var handler = new RefreshTokenCommandHandler(context, jwtTokenService.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("expired-raw-token"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
