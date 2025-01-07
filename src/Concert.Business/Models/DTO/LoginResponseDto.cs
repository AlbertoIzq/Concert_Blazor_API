using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class LoginResponseDto
    {
        public string JwtToken { get; set; }
    }
}