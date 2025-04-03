using System.Net;
using System.Text.Json;
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
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";

                var error = new
                {
                    Id = errorId,
                    ErrorMessage = "Something went wrong! We are looking into resolving this."
                };

                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }

    }
}
