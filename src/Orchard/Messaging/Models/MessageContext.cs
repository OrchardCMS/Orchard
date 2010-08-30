using System.Collections.Generic;
using System.Net.Mail;

namespace Orchard.Messaging.Models {
    public class MessageContext {
        public Dictionary<string, string> Properties { get; private set; }
        public Message Message { get; private set; }
        public MailMessage MailMessage { get; private set; }

        public MessageContext(Message message) {
            Properties = new Dictionary<string, string>();
            Message = message;
            MailMessage = new MailMessage {Body = message.Body, Subject = message.Subject};
        }
    }
}
