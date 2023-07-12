namespace SampleApp.Models
{
    public class MailModel
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string From { get; set; }
        public string MailCc { get;  set; }
        public string MailBcc { get;  set; }
    }
}
