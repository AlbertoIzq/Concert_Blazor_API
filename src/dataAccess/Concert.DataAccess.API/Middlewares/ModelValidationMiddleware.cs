using Concert.DataAccess.API.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace Concert.DataAccess.API.Middlewares
{
    public class ModelValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ModelValidationMiddleware> _logger;

        public ModelValidationMiddleware(RequestDelegate next, ILogger<ModelValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var modelType = GetModelTypeFromEndpoint(endpoint);
            if (modelType != null)
            {
                // Bind and validate the model
                var model = await BindModelAsync(httpContext, modelType);
                if (model == null)
                {
                    _logger.LogWarning("Invalid JSON payload");
                    // Terminate the pipeline
                    return;
                }

                var validationErrors = ValidateModel(model);
                if (validationErrors.Count > 0)
                {
                    await SendAndLogBadRequestResponse(httpContext, validationErrors);
                    // Terminate the pipeline
                    return;
                }
            }

            // Continue if the model is valid
            await _next(httpContext);
        }

        private async Task<object?> BindModelAsync(HttpContext httpContext, Type modelType)
        {
            try
            {
                // EnableBuffering and leaveOpen: true are used to enable rereading the stream afterwards
                httpContext.Request.EnableBuffering();
                var originalPosition = httpContext.Request.Body.Position;
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = originalPosition;

                return JsonSerializer.Deserialize(body, modelType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to deserialize request body.", ex);
                // Terminate the pipeline
                return null;
            }
        }

        private Dictionary<string, List<string>> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            // Group validation errors by property names
            var validationErrors = new Dictionary<string, List<string>>();

            foreach (var validationResult in results)
            {
                // Get the property name
                var propertyName = validationResult.MemberNames.First();
                if (!validationErrors.ContainsKey(propertyName))
                {
                    validationErrors[propertyName] = new List<string>();
                }
                validationErrors[propertyName].Add(validationResult.ErrorMessage!);
            }

            return validationErrors;
        }

        private Type? GetModelTypeFromEndpoint(Endpoint? endpoint)
        {
            if (endpoint == null) return null;

            // Retrieve metadata for the endpoint's controller action
            var actionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
            if (actionDescriptor == null) return null;

            // Find the first parameter that is bound from the body
            var bodyParameter = actionDescriptor.MethodInfo.GetParameters()
                .FirstOrDefault(p => p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromBodyAttribute>() != null);

            // If [FromBody] is not explicitly used, assume the first parameter is the model
            return bodyParameter?.ParameterType;
        }

        private async Task SendAndLogBadRequestResponse(HttpContext httpContext, Dictionary<string, List<string>> errors)
        {
            LoggerHelper<ModelValidationMiddleware>.LogCalledEndpoint(_logger, httpContext);

            var problemDetails = new CustomProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request.",
                Detail = "One or more input validation errors occurred.",
                Errors = errors
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(problemDetails);

            LoggerHelper<ModelValidationMiddleware>.LogResultEndpoint(_logger, httpContext, "Bad Request", problemDetails);
        }
    }
}