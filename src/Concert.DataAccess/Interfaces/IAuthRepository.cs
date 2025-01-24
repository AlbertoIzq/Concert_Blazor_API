using Concert.Business.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Concert.DataAccess.Interfaces
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Create a JWT token for access
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        Token CreateAccessToken(List<Claim> claims);
        /// <summary>
        /// Creates a cryptographic random number for refresh token
        /// </summary>
        /// <returns></returns>
        Token CreateRefreshToken();

        /// <summary>
        /// Gets ClaimsPrincipal from access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        ClaimsPrincipal GetClaimsPrincipalFromExpiredAccessToken(string token);
    }
}