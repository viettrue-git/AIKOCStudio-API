using AiKocStudio.Application.Auth.Commands.Login;
using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Application.UnitTests.Common;
using AiKocStudio.Domain.Entities;
using AiKocStudio.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AiKocStudio.Application.UnitTests.Auth;

public class LoginCommandTests
{
    private static User SeedUser(TestApplicationDbContext context, string email = "user@example.com")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed",
            DisplayName = "Test User",
            Role = UserRole.Member,
            IsActive = true,
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndPersistsRefreshToken()
    {
        using var context = TestApplicationDbContext.Create();
        var user = SeedUser(context);

        var identityService = new Mock<IIdentityService>();
        identityService.Setup(s => s.VerifyPassword(It.IsAny<User>(), "correct-password")).Returns(true);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        jwtTokenService.Setup(s => s.GenerateRefreshToken()).Returns("raw-refresh-token");
        jwtTokenService.Setup(s => s.HashRefreshToken("raw-refresh-token")).Returns("hashed-refresh-token");

        var handler = new LoginCommandHandler(context, identityService.Object, jwtTokenService.Object);

        var result = await handler.Handle(new LoginCommand(user.Email, "correct-password"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");

        var persisted = await context.RefreshTokens.SingleAsync();
        persisted.UserId.Should().Be(user.Id);
        persisted.TokenHash.Should().Be("hashed-refresh-token");
        persisted.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsAuthenticationFailedException()
    {
        using var context = TestApplicationDbContext.Create();
        var user = SeedUser(context);

        var identityService = new Mock<IIdentityService>();
        identityService.Setup(s => s.VerifyPassword(It.IsAny<User>(), It.IsAny<string>())).Returns(false);

        var jwtTokenService = new Mock<IJwtTokenService>();
        var handler = new LoginCommandHandler(context, identityService.Object, jwtTokenService.Object);

        var act = () => handler.Handle(new LoginCommand(user.Email, "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
