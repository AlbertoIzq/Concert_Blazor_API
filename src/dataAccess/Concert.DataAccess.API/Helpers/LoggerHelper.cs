using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Helpers
{
    public static class LoggerHelper<T> where T : class
    {
        public static void LogCalledEndpoint(ILogger<T> logger, HttpContext httpContext)
        {
            logger.LogInformation("Called endpoint '{method}', '{endpoint}'",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        public static void LogResultEndpoint(ILogger<T> logger, HttpContext httpContext, string result, object response)
        {
            // Instead of @response you could use as a param JsonSerializer.Serialize(response))
            // but then the type is not saved in the json
            logger.LogInformation("Result endpoint '{method}', '{endpoint}': '{result}', response: {@response}",
                httpContext.Request.Method, httpContext.Request.Path, result, response);
        }
    }
}