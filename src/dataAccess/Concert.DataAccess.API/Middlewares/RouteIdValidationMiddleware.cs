using Concert.Business.Models.Domain;
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
                    _logger.LogInformation("Called endpoint '{method}', '{endpoint}'",
                        httpContext.Request.Method, httpContext.Request.Path);

                    var problemDetails = new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Bad Request.",
                        Detail = $"Id '{id}' must be an int."
                    };

                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await httpContext.Response.WriteAsJsonAsync(problemDetails);

                    _logger.LogInformation("Result endpoint '{method}', '{endpoint}': '{result}', response: {@problemDetails}",
                        httpContext.Request.Method, httpContext.Request.Path, "Bad Request", problemDetails);

                    return;
                }
            }
            await _next(httpContext);
        }
    }
}