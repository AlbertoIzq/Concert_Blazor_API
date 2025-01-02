using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Concert.UIWasm.Data
{
    public class WebApiException : Exception
    {
        public ProblemDetails? ProblemDetails { get; set; }

        public WebApiException(string errorJson)
        { 
            ProblemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorJson);
        }
    }
}