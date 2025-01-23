namespace Concert.Business.Models.Domain
{
    public class LoginRefreshResponseDto
    {
        public Token AccessToken { get; set; }
        public Token RefreshToken { get; set; }
    }
}