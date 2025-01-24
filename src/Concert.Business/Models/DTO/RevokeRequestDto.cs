using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class RevokeRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}