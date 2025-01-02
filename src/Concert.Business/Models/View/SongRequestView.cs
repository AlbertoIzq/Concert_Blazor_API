using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.View
{
    public class SongRequestView
    {
        public int Id { get; set; }
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