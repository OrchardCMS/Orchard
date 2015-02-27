namespace Orchard.Email.Models {
    public class EmailMessage {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Recipients { get; set; }
        public string ReplyTo { get; set; }
        public string Bcc { get; set; }
        public string CC { get; set; }
    }
}