using Concert.Business.Constants;
using Concert.Business.Models;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Concert.DataAccess.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        public Token CreateAccessToken(List<Claim> claims)
        {
            // Read environment variables.
            new EnvLoader().Load();
            var envVarReader = new EnvReader();
            string jwtSecretKey = envVarReader["Jwt_SecretKey"];
            string jwtIssuer = envVarReader["Jwt_Issuer"];
            string jwtAudience = envVarReader["Jwt_Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAtDate = DateTime.Now.AddMinutes(BackConstants.ACCESS_TOKEN_EXPIRATION_MINUTES);
            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtAudience,
                claims,
                expires: expiresAtDate,
                signingCredentials: credentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return new Token()
            {
                Value = tokenValue,
                ExpiresAt = expiresAtDate
            };
        }

        public Token CreateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var expiresAtDate = DateTime.Now.AddHours(BackConstants.REFRESH_TOKEN_EXPIRATION_HOURS);
            var tokenValue = Convert.ToBase64String(randomNumber);

            return new Token()
            {
                Value = tokenValue,
                ExpiresAt = expiresAtDate,
            };
        }

        public ClaimsPrincipal GetClaimsPrincipalFromExpiredAccessToken(string token)
        {
            // Read environment variables.
            new EnvLoader().Load();
            var envVarReader = new EnvReader();
            string jwtSecretKey = envVarReader["Jwt_SecretKey"];
            string jwtIssuer = envVarReader["Jwt_Issuer"];
            string jwtAudience = envVarReader["Jwt_Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidIssuer = jwtIssuer,
                ValidAudiences = new[] { jwtAudience },
                ValidateLifetime = false // Token expiration date isn't important
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}