namespace SampleApp.Models
{
    public class AccessTokenModel
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string UserName { get; set; }

        public string Token_type { get; set; }

        public DateTime ExpiresOn { get; set; }
    }
}
