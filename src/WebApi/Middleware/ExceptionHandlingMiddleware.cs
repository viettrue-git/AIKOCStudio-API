using System.Net;
using AiKocStudio.Application.Common.Exceptions;
using ValidationException = AiKocStudio.Application.Common.Exceptions.ValidationException;

namespace AiKocStudio.WebApi.Middleware;

/// <summary>
/// Minimal exception-to-status-code mapping so Application-layer failures
/// (validation, not-found, forbidden, unauthenticated) surface as the right
/// HTTP status instead of a raw 500. Phase 7 revisits this for a fuller,
/// consistent error response shape across the whole API.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed", ex.Errors);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, "Authentication required.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, object? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new { title, status = (int)statusCode, errors });
    }
}
