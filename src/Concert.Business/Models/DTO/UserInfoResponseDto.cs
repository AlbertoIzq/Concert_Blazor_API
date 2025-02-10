using System.Security.Claims;

namespace Concert.Business.Models.Domain
{
    public class UserInfoResponseDto
    {
        public string Name { get; set; }
        public List<string> Roles { get; set; }
        public List<UserClaimDto> Claims { get; set; }
    }
}