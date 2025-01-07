using Microsoft.AspNetCore.Identity;

namespace Concert.DataAccess.Interfaces
{
    public interface IAuthRepository
    {
        public string CreateJWTToken(IdentityUser identityUser, List<string> roles);
    }
}