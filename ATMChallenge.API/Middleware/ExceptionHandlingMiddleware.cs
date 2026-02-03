using System.Net;
using System.Text.Json;

namespace ATMChallenge.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException knf)
            {
                _logger.LogWarning(knf, "Not found");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                var result = JsonSerializer.Serialize(new { code = 404, message = knf.Message });
                await context.Response.WriteAsync(result);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogWarning(ua, "Unauthorized");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var result = JsonSerializer.Serialize(new { code = 401, message = ua.Message });
                await context.Response.WriteAsync(result);
            }
            catch (FluentValidation.ValidationException fv)
            {
                _logger.LogWarning(fv, "Validation failed");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = fv.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage });
                var result = JsonSerializer.Serialize(new { code = 400, message = "Validation error", details = errors });
                await context.Response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var result = JsonSerializer.Serialize(new { code = 500, message = "An unexpected error occurred." });
                await context.Response.WriteAsync(result);
            }
        }
    }
}
