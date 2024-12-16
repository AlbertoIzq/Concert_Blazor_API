using Concert.DataAccess.API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Middlewares
{
    public class RouteIdValidationMiddleware
    {
        private readonly ILogger<RouteIdValidationMiddleware> _logger;
        private readonly RequestDelegate _next;

        public RouteIdValidationMiddleware(ILogger<RouteIdValidationMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue && httpContext.Request.Path.Value.StartsWith("/api/"))
            {
                var id = httpContext.Request.RouteValues["id"]?.ToString();
                if (id is not null && !int.TryParse(id, out _))
                {
                    LoggerHelper<RouteIdValidationMiddleware>.LogCalledEndpoint(_logger, httpContext);

                    var problemDetails = new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Bad Request.",
                        Detail = $"Id '{id}' must be an int."
                    };

                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsJsonAsync(problemDetails);

                    LoggerHelper<RouteIdValidationMiddleware>.LogResultEndpoint(_logger, httpContext, "Bad Request", problemDetails);

                    // Terminate the pipeline
                    return;
                }
            }
            // Continue if the id is valid
            await _next(httpContext);
        }
    }
}