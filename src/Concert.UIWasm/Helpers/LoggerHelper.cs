using Serilog;

namespace Concert.UIWasm.Helpers
{
    public static class LoggerHelper
    {
        public static void LogException(string message, Exception ex)
        {
            // Done this way because Serilog doesn't support proper serialization of Exception
            var exception = ex.ToString();

            Log.Logger.Error("Exception message: {message}\n" +
                "Exception: {exception}",
                message, exception);
        }

        public static void LogWebApiException(string message, string requestPath,
            HttpMethod httpMethod, int status, Exception ex)
        {
            // Done this way because Serilog doesn't support proper serialization of Exception
            var exception = ex.ToString();

            Log.Logger.Error("Exception message: {ex.Message}\n" +
                "RequestPath: {requestPath}\n" +
                "RequestMethod: {httpMethod}\n" +
                "Status: {status}\n" +
                "Exception: {exception},",                
                message, requestPath, httpMethod.Method, status, exception);
        }
    }
}