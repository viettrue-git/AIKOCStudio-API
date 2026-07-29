using AiKocStudio.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiKocStudio.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            typeof(TRequest).Name,
            currentUserService.UserId);

        return next();
    }
}
