using System.Net;
using ExploreGambia.API.Exceptions;

namespace ExploreGambia.API.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;
        private readonly RequestDelegate next;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
            RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();

                logger.LogError(ex, "Unhandled exception occurred. ErrorId: {ErrorId}", errorId);

                var statusCode = HttpStatusCode.InternalServerError;
                var errorCode = "internal_server_error";
                var errorMessage = "Something went wrong. We are looking into resolving this.";

                if (ex is BookingNotFoundException
                    || ex is TourNotFoundException
                    || ex is TourGuideNotFoundException
                    || ex is PaymentNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    errorCode = "resource_not_found";
                    errorMessage = ex.Message;
                }
                else if (ex is BusinessRuleException)
                {
                    statusCode = HttpStatusCode.Conflict;
                    errorCode = "business_rule_violation";
                    errorMessage = ex.Message;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    statusCode = HttpStatusCode.Forbidden;
                    errorCode = "forbidden";
                    errorMessage = ex.Message;
                }

                httpContext.Response.StatusCode = (int)statusCode;
                httpContext.Response.ContentType = "application/json";

                var error = new
                {
                    ErrorId = errorId,
                    Code = errorCode,
                    Message = errorMessage,
                    Details = Array.Empty<string>()
                };

                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }
    }
}
