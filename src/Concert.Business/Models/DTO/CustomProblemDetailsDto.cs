namespace Concert.Business.Models.Domain
{
    public class CustomProblemDetailsDto
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
        public string? Type { get; set; }
        public string? Instance { get; set; }
        public Dictionary<string, List<string>>? Errors { get; set; } = new();
    }
}