using Microsoft.AspNetCore.Diagnostics;

namespace SmartAudit.Api.Middleware
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            if (context.Response.HasStarted)
                return false;

            var (statusCode, message) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Chyba na straně serveru")
            };

            _logger.LogError(exception, "Neošetřená chyba na {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new { Message = message }, ct);

            return true;
        }
    }
}
