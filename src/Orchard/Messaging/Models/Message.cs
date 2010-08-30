using Orchard.ContentManagement.Records;

namespace Orchard.Messaging.Models {
    public class Message {
        public string Service { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
        public ContentItemRecord Recipient { get; set; }
    }
}
