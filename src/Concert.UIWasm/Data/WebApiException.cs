using System.Text.Json;

namespace Concert.UIWasm.Data
{
    public class WebApiException : Exception
    {
        public CustomProblemDetails? ProblemDetails { get; set; }

        public WebApiException(string errorJson)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ProblemDetails = JsonSerializer.Deserialize<CustomProblemDetails>(errorJson, options);
        }
    }
}