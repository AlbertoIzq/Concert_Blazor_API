using Concert.Business.Constants;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Concert.DataAccess.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        public string CreateJWTToken(IdentityUser identityUser, List<string> roles)
        {
            // Create claims
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, identityUser.Email));
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Read environment variables.
            new EnvLoader().Load();
            var envVarReader = new EnvReader();
            string jwtSecretKey = envVarReader["Jwt_SecretKey"];
            string jwtIssuer = envVarReader["Jwt_Issuer"];
            string jwtAudience = envVarReader["Jwt_Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtAudience,
                claims,
                expires: DateTime.Now.AddMinutes(BackConstants.JWT_TOKEN_EXPIRATION_MINUTES));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}