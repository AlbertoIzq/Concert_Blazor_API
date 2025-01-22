using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class LoginResponseDto
    {
        public Token AccessToken { get; set; }
        public Token RefreshToken { get; set; }
    }
}