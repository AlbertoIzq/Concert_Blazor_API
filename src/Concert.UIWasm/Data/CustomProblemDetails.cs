namespace Concert.UIWasm.Data
{
    public class CustomProblemDetails
    {
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public CustomProblemDetails()
        {
            Errors = new Dictionary<string, List<string>>();
        }
    }
}
