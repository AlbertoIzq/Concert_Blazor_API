using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class SongRequest : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Artist { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(30)]
        public string Genre { get; set; }
        [Required]
        [MaxLength(30)]
        public string Language { get; set; }
    }
}