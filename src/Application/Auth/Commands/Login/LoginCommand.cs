using AiKocStudio.Application.Common.Models;
using MediatR;

namespace AiKocStudio.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
