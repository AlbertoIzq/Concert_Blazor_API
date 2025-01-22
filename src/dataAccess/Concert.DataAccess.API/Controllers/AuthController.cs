using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Filters.ActionFilters;
using Concert.DataAccess.API.Helpers;
using Concert.DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager, IAuthRepository authRepository,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/auth/Register
        /// </summary>
        /// <param name="registerRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
        [TypeFilter(typeof(AuthRegisterRolesValidationFilterAttribute))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            var identityUser = new IdentityUser()
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username
            };
            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);
            if (identityResult.Succeeded)
            {
                // Add roles to the user
                identityResult = await _userManager.AddToRolesAsync(identityUser, registerRequestDto.Roles);

                if (identityResult.Succeeded)
                {
                    var okMessage = "User was registered succesfully! Please login";
                    LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Ok", okMessage);
                    return Ok(new { result = okMessage });
                }
            }

            var problemDetails = GetRegisterErrors(identityResult);

            LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Bad Request", problemDetails);
            return BadRequest(problemDetails);
        }

        /// <summary>
        /// POST:api/auth/Login
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            var identityUser = await _userManager.FindByEmailAsync(loginRequestDto.Username);

            if (identityUser is not null)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(identityUser, loginRequestDto.Password);

                if (checkPasswordResult)
                {
                    // Get roles for this user
                    var roles = await _userManager.GetRolesAsync(identityUser);

                    if (roles is not null)
                    {
                        // Create JWT Token
                        var jwtToken = _authRepository.CreateJWTToken(identityUser, roles.ToList());
                        var response = new LoginResponseDto
                        {
                            AccessToken = jwtToken
                        };

                        LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Ok", response);
                        return Ok(response);
                    }
                }
            }

            var problemDetails = new ProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad credentials",
                Detail = "Username or password are incorrect."
            };
            LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Bad Request", problemDetails);
            return BadRequest(problemDetails);
        }

        /// <summary>
        /// Get a list of obtained errors when trying to register the user
        /// </summary>
        /// <param name="identityResult"></param>
        /// <returns>ProblemDetails with errors from the unsuccessful registration</returns>
        private CustomProblemDetails GetRegisterErrors(IdentityResult identityResult)
        {
            var errors = identityResult.Errors.Select(e => e.Description);
            var problemDetails = new CustomProblemDetails();
            if (errors is not null && errors.Count() > 0)
            {
                var errorsProblemDetails = new Dictionary<string, List<string>>();
                var passwordErrorsList = new List<string>();
                foreach (var error in errors)
                {
                    if (error.Contains("Password"))
                    {
                        passwordErrorsList.Add(error);
                    }
                    else
                    {
                        errorsProblemDetails.Add(error, new List<string>());
                    }
                }

                if (passwordErrorsList.Count() > 0)
                {
                    errorsProblemDetails.Add("Password", passwordErrorsList);
                }

                problemDetails.Errors = errorsProblemDetails;
            }

            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Bad Request.";
            problemDetails.Detail = "Something went wrong.";

            return problemDetails;
        }
    }
}