using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string[] Roles { get; set; }
    }
}