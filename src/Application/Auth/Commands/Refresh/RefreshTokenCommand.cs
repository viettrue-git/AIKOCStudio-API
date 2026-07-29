using AiKocStudio.Application.Common.Models;
using MediatR;

namespace AiKocStudio.Application.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;
