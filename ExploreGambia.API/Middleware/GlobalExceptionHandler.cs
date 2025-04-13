using System.Net;
using System.Text.Json;
using ExploreGambia.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;

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

                // Log This Exception
                logger.LogError(ex, $"{errorId} : {ex.Message}");

                // Return A Custom Exrror Response
                // Determine the HTTP status code based on the exception type
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                string errorMessage = "Something went wrong! We are looking into resolving this.";



                if (ex is BookingNotFoundException || ex is TourNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = ex.Message;
                }

                // Return a custom error response
                httpContext.Response.StatusCode = (int)statusCode;
                httpContext.Response.ContentType = "application/json";

                var error = new
                {
                    Id = errorId,
                    ErrorMessage = errorMessage
                };

                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }

    }
}
