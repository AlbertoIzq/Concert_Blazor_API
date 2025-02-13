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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityUser> userManager, IAuthRepository authRepository,
            IRefreshTokenRepository refreshTokenRepository, IEncryptionService encryptionService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/auth/register
        /// Register a new user with Reader role
        /// </summary>
        /// <param name="registerRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            var identityUser = new IdentityUser()
            {
                UserName = registerRequestDto.UserEmail,
                Email = registerRequestDto.UserEmail
            };
            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);
            if (identityResult.Succeeded)
            {
                // Add Reader role to the user
                identityResult = await _userManager.AddToRoleAsync(identityUser, BackConstants.READER_ROLE_NAME);

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
        /// POST: api/auth/create-user
        /// </summary>
        /// <param name="createUserRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(AuthRegisterRolesValidationFilterAttribute))]
        [Authorize(Roles = BackConstants.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> CreateUser(CreateUserRequestDto createUserRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            var identityUser = new IdentityUser()
            {
                UserName = createUserRequestDto.UserEmail,
                Email = createUserRequestDto.UserEmail
            };
            var identityResult = await _userManager.CreateAsync(identityUser, createUserRequestDto.Password);
            if (identityResult.Succeeded)
            {
                // Add Reader role to the user
                identityResult = await _userManager.AddToRolesAsync(identityUser, createUserRequestDto.Roles);

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
        /// POST: api/auth/login
        /// Login to get access and refresh tokens.
        /// If frontend call, tokens are added in response as HTTP-only cookies
        /// and if API call, they are added in the response body
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <param name="apiRequest">Header parameter "Api-Request", true if API call</param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto,
            [FromHeader(Name = "Api-Request")] string? apiRequest)
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
                        var claims = CreateClaimsForTokenGeneration(identityUser.Id, identityUser.Email, (List<string>)roles);
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

                        // Return tokens only for API clients
                        if (!string.IsNullOrEmpty(apiRequest) && apiRequest == "true")
                        {
                            return Ok(response);
                        }
                        // Set secure HTTP-only cookies for frontend
                        else
                        {
                            // Cookie for JWT token
                            Response.Cookies.Append(BackConstants.JWT_TOKEN_COOKIE_NAME,
                                response.AccessToken.Value, new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.None, // Required for cross-origin requests
                                    Expires = response.AccessToken.ExpiresAt,
                                    Path = "/"
                                });

                            // Cookie for refresh token
                            Response.Cookies.Append(BackConstants.REFRESH_TOKEN_COOKIE_NAME,
                                response.RefreshToken.Value, new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.None,
                                    Expires = response.RefreshToken.ExpiresAt,
                                    Path = "/"
                                });

                            return Ok(new { message = "User was successfully logged in!" });
                        }
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
        /// POST: api/auth/logout
        /// Logout by deleting cookies
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            if (Request.Cookies[BackConstants.JWT_TOKEN_COOKIE_NAME] is not null)
            {
                Response.Cookies.Delete(BackConstants.JWT_TOKEN_COOKIE_NAME);
            }

            if (Request.Cookies[BackConstants.REFRESH_TOKEN_COOKIE_NAME] is not null)
            {
                Response.Cookies.Delete(BackConstants.REFRESH_TOKEN_COOKIE_NAME);
            }

            var okMessage = "User was logged out succesfully!";
            LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Ok", okMessage);

            return Ok(new { result = okMessage });
        }

        /// <summary>
        /// POST:api/auth/refresh
        /// Refresh refresh token
        /// </summary>
        /// <param name="refreshRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh(RefreshRequestDto? refreshRequestDto,
            [FromHeader(Name = "Api-Request")] string? apiRequest)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            string refreshToken = string.Empty;
            var badRequestDetail = string.Empty;

            // If API client, read token value from body
            if (!string.IsNullOrEmpty(apiRequest) && apiRequest == "true")
            {
                refreshToken = refreshRequestDto.RefreshToken;
            }
            // If frontend call, read token value from cookies
            else
            {
                if (Request.Cookies[BackConstants.REFRESH_TOKEN_COOKIE_NAME] is not null)
                {
                    refreshToken = Request.Cookies[BackConstants.REFRESH_TOKEN_COOKIE_NAME];
                }
            }

            RefreshToken? refreshTokenDb = null;

            // Validate the refresh token
            if (string.IsNullOrEmpty(refreshToken))
            {
                badRequestDetail = "Refresh token is missing.";
            }
            else
            {
                var refreshTokenEncryptedValue = _encryptionService.Encrypt(refreshToken);
                refreshTokenDb = await _refreshTokenRepository.GetByTokenValueAsync(refreshTokenEncryptedValue);

                if (refreshTokenDb is null)
                {
                    badRequestDetail = "Refresh token invalid or user has no refresh token.";
                }
                else
                {
                    if (refreshTokenDb.ExpiryDate <= DateTime.Now)
                    {
                        badRequestDetail = "Refresh token is expired.";
                    }
                }
            }

            // Send a BadRequest if any validation problem found
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

            var identityUser = await _userManager.FindByNameAsync(refreshTokenDb.UserName);
            var roles = await _userManager.GetRolesAsync(identityUser);
            var claims = new List<Claim>();
            
            if (roles is not null)
            {
                claims = CreateClaimsForTokenGeneration(identityUser.Id, identityUser.Email, (List<string>)roles);
            }
            else
            {
                claims = CreateClaimsForTokenGeneration(identityUser.Id, identityUser.Email, new List<string>());
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

            // Return tokens only for API clients
            if (!string.IsNullOrEmpty(apiRequest) && apiRequest == "true")
            {
                return Ok(response);
            }
            // Set secure HTTP-only cookies for frontend
            else
            {
                // Cookie for JWT token
                Response.Cookies.Append(BackConstants.JWT_TOKEN_COOKIE_NAME,
                    response.AccessToken.Value, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None, // Required for cross-origin requests
                        Expires = response.AccessToken.ExpiresAt,
                        Path = "/"
                    });

                // Cookie for refresh token
                Response.Cookies.Append(BackConstants.REFRESH_TOKEN_COOKIE_NAME,
                    response.RefreshToken.Value, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = response.RefreshToken.ExpiresAt,
                        Path = "/"
                    });

                return Ok(new { message = "User tokens were successfully refreshed!" });
            }  
        }

        /// <summary>
        /// POST: api/auth/revoke
        /// Revoke refresh token. User can only revoke his own refresh token,
        /// except Admin user who can revoke any
        /// </summary>
        /// <param name="revokeRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<IActionResult> Revoke(RevokeRequestDto revokeRequestDto)
        {
            LoggerHelper<AuthController>.LogCalledEndpoint(_logger, HttpContext);

            // Get the currently authenticated user
            var userName = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var refreshTokenEncryptedValue = _encryptionService.Encrypt(revokeRequestDto.RefreshToken);
            var refreshTokenToBeDeleted = await _refreshTokenRepository.GetByTokenValueAsync(refreshTokenEncryptedValue);

            if (refreshTokenToBeDeleted == null)
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Item not found.",
                    Detail = "The refresh token couldn't be found."
                };

                LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "Not Found", problemDetails);
                return NotFound(problemDetails);
            }

            // Check if the user is an admin
            bool isAdmin = userRoles.Contains(BackConstants.ADMIN_ROLE_NAME);

            // Check if the user is revoking their own token
            bool isOwner = refreshTokenToBeDeleted.UserName == userName;

            // Admin can revoke all tokens, and no admin users can only revoke their own token
            if (!isAdmin && !isOwner)
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden.",
                    Detail = "You aren't authorized to revoke this refresh token."
                };

                // Return a 403 Forbidden response
                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            var refreshTokenDeleted = await _refreshTokenRepository.DeleteAsync(refreshTokenToBeDeleted.Id);
            LoggerHelper<AuthController>.LogResultEndpoint(_logger, HttpContext, "No Content");

            // Return No content back to client
            return NoContent();
        }

        /// <summary>
        /// GET: api/auth/user-info
        /// Get authenticated user name and roles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("user-info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = HttpContext.User;

            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userInfo = new UserInfoResponseDto()
            {
                Name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value,
                Roles = user.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList(),
                Claims = user.Claims
                    .Select(c => new UserClaimDto { Type = c.Type, Value = c.Value })
                    .ToList()
            };

            return Ok(userInfo);
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

            var refreshTokenEncryptedValue = _encryptionService.Encrypt(refreshToken.Value);

            // Manage refresh token
            var refreshTokenEntity = new RefreshToken()
            {
                Value = refreshTokenEncryptedValue,
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
        /// Create Email claim, NameIdentifier claim (user Id) and Roles claims for access token generation
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private List<Claim> CreateClaimsForTokenGeneration(string userId, string userEmail, List<string> roles)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Email, userEmail));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}