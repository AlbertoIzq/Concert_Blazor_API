using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}