using Concert.Business.Models;
using Microsoft.AspNetCore.Identity;

namespace Concert.DataAccess.Interfaces
{
    public interface IAuthRepository
    {
        public Token CreateJWTToken(IdentityUser identityUser, List<string> roles);
    }
}