using System.Security.Claims;

namespace Concert.Business.Models.Domain
{
    public class UserClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}