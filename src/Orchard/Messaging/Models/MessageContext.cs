using System.Collections.Generic;
using System.Net.Mail;
using Orchard.ContentManagement.Records;

namespace Orchard.Messaging.Models {
    public class MessageContext {
        public MailMessage MailMessage { get; private set; }
        public string Type { get; set; }
        public string Service { get; set; }
        public ContentItemRecord Recipient { get; set; }
        public Dictionary<string, string> Properties { get; private set; }
        public bool MessagePrepared { get; set; }

        public MessageContext() {
            Properties = new Dictionary<string, string>();
            MailMessage = new MailMessage();
        }
    }
}
