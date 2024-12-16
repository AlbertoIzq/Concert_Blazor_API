using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Helpers
{
    public class CustomProblemDetails : ProblemDetails
    {
        public Dictionary<string, List<string>> Errors { get; set; }

        public CustomProblemDetails()
        {
            Errors = new Dictionary<string, List<string>>();
        }
    }
}