using Concert.Business.Constants;
using Concert.Business.Models;
using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Filters.ActionFilters;
using Concert.DataAccess.API.Helpers;
using Concert.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace Concert.DataAccess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager, IAuthRepository authRepository,
            IRefreshTokenRepository refreshTokenRepository, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _refreshTokenRepository = refreshTokenRepository;
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
                    var roles = await _userManager.GetRolesAsync(identityUser);

                    if (roles is not null)
                    {
                        var claims = CreateClaimsForTokenGeneration(loginRequestDto.Username, (List<string>)roles);
                        var response = await CreateTokens(claims);

                        var responseForLogging = new LoginRefreshResponseDto()
                        {
                            AccessToken = new Token()
                            {
                                Value = "..." +
                                    response.AccessToken.Value.Substring(response.AccessToken.Value.Length - BackConstants.TOKEN_LAST_NUM_CHARS_LOGGING),
                                ExpiresAt = response.AccessToken.ExpiresAt
                            },
                            RefreshToken = new Token()
                            {
                                Value = "..." +
                                    response.RefreshToken.Value.Substring(response.RefreshToken.Value.Length - BackConstants.TOKEN_LAST_NUM_CHARS_LOGGING),
                                ExpiresAt = response.RefreshToken.ExpiresAt
                            }
                        };
                        
                        LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Ok", responseForLogging);
                        
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
        /// POST:api/auth/Refresh
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh(RefreshRequestDto refreshRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            string accessToken = refreshRequestDto.AccessToken;
            string refreshToken = refreshRequestDto.RefreshToken;

            var claimsPrincipal = _authRepository.GetClaimsPrincipalFromExpiredAccessToken(accessToken);
            var username = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

            var refreshTokenDb = await _refreshTokenRepository.GetByUserNameAsync(username);
            var badRequestDetail = string.Empty;

            // Check if refresh token is correct
            if (refreshTokenDb is null)
            {
                badRequestDetail = "User has no refresh token.";
            }
            else if (refreshTokenDb.Value != refreshRequestDto.RefreshToken)
            {
                badRequestDetail = "Refresh token is invalid.";
            }
            else if (refreshTokenDb.ExpiryDate <= DateTime.Now)
            {
                badRequestDetail = "Refresh token is expired.";
            }

            if (!string.IsNullOrEmpty(badRequestDetail))
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid client request",
                    Detail = badRequestDetail
                };
                LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Bad Request", problemDetails);
                return BadRequest(problemDetails);
            }

            // Extract claims from access token
            var claims = new List<Claim>();
            foreach (var claim in claimsPrincipal.Claims)
            {
                if (claim.Type == ClaimTypes.Email || claim.Type == ClaimTypes.Role)
                {
                    claims.Add(claim);
                }
            }

            var response = await CreateTokens(claims);

            var responseForLogging = new LoginRefreshResponseDto()
            {
                AccessToken = new Token()
                {
                    Value = "..." +
                                    response.AccessToken.Value.Substring(response.AccessToken.Value.Length - BackConstants.TOKEN_LAST_NUM_CHARS_LOGGING),
                    ExpiresAt = response.AccessToken.ExpiresAt
                },
                RefreshToken = new Token()
                {
                    Value = "..." +
                                    response.RefreshToken.Value.Substring(response.RefreshToken.Value.Length - BackConstants.TOKEN_LAST_NUM_CHARS_LOGGING),
                    ExpiresAt = response.RefreshToken.ExpiresAt
                }
            };

            LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Ok", responseForLogging);
            return Ok(response);      
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

        /// <summary>
        /// Create access token and refresh token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        private async Task<LoginRefreshResponseDto> CreateTokens(List<Claim> claims)
        {
            var userName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

            // Create tokens
            var accessToken = _authRepository.CreateAccessToken(claims);
            var refreshToken = _authRepository.CreateRefreshToken();

            // Manage refresh token
            var refreshTokenEntity = new RefreshToken()
            {
                Value = refreshToken.Value,
                ExpiryDate = refreshToken.ExpiresAt,
                UserName = userName
            };

            var refreshTokenDb = await _refreshTokenRepository.GetByUserNameAsync(userName);

            if (refreshTokenDb is null)
            {
                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);
            }
            else
            {
                await _refreshTokenRepository.UpdateAsync(refreshTokenDb.Id, refreshTokenEntity);
            }

            return new LoginRefreshResponseDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        /// <summary>
        /// Create Email claim and Roles claims for access token generation
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private List<Claim> CreateClaimsForTokenGeneration(string userEmail, List<string> roles)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Email, userEmail));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}