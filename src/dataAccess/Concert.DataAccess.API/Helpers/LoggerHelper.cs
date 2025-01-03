namespace Concert.DataAccess.API.Helpers
{
    public static class LoggerHelper<T> where T : class
    {
        public static void LogCalledEndpoint(ILogger<T> logger, HttpContext httpContext)
        {
            logger.LogInformation("Called endpoint '{method}', '{endpoint}'",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        public static void LogResultEndpoint(ILogger<T> logger, HttpContext httpContext, string result, object response = null)
        {
            // Instead of @response you could use as a param JsonSerializer.Serialize(response))
            // but then the type is not saved in the json
            if (response is not null)
            {
                logger.LogInformation("Result endpoint '{method}', '{endpoint}': '{result}', response: {@response}",
                httpContext.Request.Method, httpContext.Request.Path, result, response);
            }
            else
            {
                logger.LogInformation("Result endpoint '{method}', '{endpoint}': '{result}'",
                httpContext.Request.Method, httpContext.Request.Path, result);
            }
        }

        public static void LogException(ILogger<T> logger, HttpContext httpContext, Exception ex)
        {
            logger.LogError("Exception message: {ex.Message}, Exception: {@ex}," +
                "RequestMethod: {@httpContext.Request.Method}, RequestPath: {@httpContext.Request.Path}",
                ex.Message, ex, httpContext.Request.Method, httpContext.Request.Path);
        }
    }
}