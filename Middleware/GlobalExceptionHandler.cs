using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var error = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Detail = exception.StackTrace
        };

        await context.Response.WriteAsJsonAsync(error);
        return true;
    }
}