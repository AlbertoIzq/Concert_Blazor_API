using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class AddSongRequestDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Artist name has to be a maximum of 100 characters.")]
        public string Artist { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Title has to be a maximum of 100 characters.")]
        public string Title { get; set; }
        [Required]
        [MaxLength(30, ErrorMessage = "Genre name has to be a maximum of 30 characters.")]
        public string Genre { get; set; }
        [Required]
        [MaxLength(30, ErrorMessage = "Language name has to be a maximum of 30 characters.")]
        public string Language { get; set; }
    }
}