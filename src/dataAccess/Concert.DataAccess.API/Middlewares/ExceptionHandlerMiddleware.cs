using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Concert.DataAccess.API.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            _logger.LogError("Exception: {ex.Message}, Exception: {@ex}," +
                "RequestMethod: {@httpContext.Request.Method}, RequestPath: {@httpContext.Request.Path}",
                ex.Message, ex, httpContext.Request.Method, httpContext.Request.Path);

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = $"Method: {httpContext.Request.Method}, Path: {httpContext.Request.Path}"
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return httpContext.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}