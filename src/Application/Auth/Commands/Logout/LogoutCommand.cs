using MediatR;

namespace AiKocStudio.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
