using Concert.Business.Constants;
using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Helpers;
using Concert.DataAccess.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Concert.DataAccess.API.Filters.ActionFilters
{
    /// <summary>
    /// Used to handle problems when registering a user with roles
    /// </summary>
    public class AuthRegisterRolesValidationFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<AuthRegisterRolesValidationFilterAttribute> _logger;

        public AuthRegisterRolesValidationFilterAttribute(ILogger<AuthRegisterRolesValidationFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var createUserRequestDto = context.ActionArguments["createUserRequestDto"] as CreateUserRequestDto;

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "It was not possible to register the user."
            };

            if (createUserRequestDto is not null)
            {
                if (!createUserRequestDto.Roles.Any() || string.IsNullOrEmpty(createUserRequestDto.Roles.FirstOrDefault()))
                {
                    problemDetails.Detail = "There were no roles provided.";
                }
                else
                {
                    foreach (var role in createUserRequestDto.Roles)
                    {
                        if (role != BackConstants.READER_ROLE_NAME &&
                            role != BackConstants.WRITER_ROLE_NAME &&
                            role != BackConstants.ADMIN_ROLE_NAME)
                        {
                            problemDetails.Detail = "There were no available roles provided.";
                        }
                    }
                }
            }

            if (problemDetails.Detail is not null)
            {
                context.Result = new BadRequestObjectResult(problemDetails);
                LoggerHelper<AuthRegisterRolesValidationFilterAttribute>.LogCalledEndpoint(_logger, context.HttpContext);
                LoggerHelper<AuthRegisterRolesValidationFilterAttribute>.LogResultEndpoint(_logger, context.HttpContext, "Bad Request", problemDetails);
            }
        }
    }
}