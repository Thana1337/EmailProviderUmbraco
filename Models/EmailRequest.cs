namespace EmailProvider.Models
{
    public class EmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string PlainText { get; set; }
    }
}
