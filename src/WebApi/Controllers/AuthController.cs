using AiKocStudio.Application.Auth.Commands.Login;
using AiKocStudio.Application.Auth.Commands.Logout;
using AiKocStudio.Application.Auth.Commands.Refresh;
using AiKocStudio.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiKocStudio.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender mediator) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(command, cancellationToken));
        }
        catch (AuthenticationFailedException)
        {
            return Unauthorized();
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(command, cancellationToken));
        }
        catch (AuthenticationFailedException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
