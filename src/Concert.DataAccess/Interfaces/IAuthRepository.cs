using Concert.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace Concert.DataAccess.Interfaces
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Create a JWT token for access
        /// </summary>
        /// <param name="identityUser"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public Token CreateAccessToken(IdentityUser identityUser, List<string> roles);
        /// <summary>
        /// Creates a cryptographic random number for refresh token
        /// </summary>
        /// <returns></returns>
        public Token CreateRefreshToken();
    }
}