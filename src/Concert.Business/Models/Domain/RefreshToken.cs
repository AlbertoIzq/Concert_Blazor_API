namespace Concert.Business.Models.Domain
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Value { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}