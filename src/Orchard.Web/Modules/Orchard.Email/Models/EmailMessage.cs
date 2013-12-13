namespace Orchard.Email.Models {
    public class EmailMessage {
        public EmailMessage() {
            
        }

        public EmailMessage(string subject, string body) {
            Subject = subject;
            Body = body;
        }

        public string Subject { get; set; }
        public string Body { get; set; }
    }
}