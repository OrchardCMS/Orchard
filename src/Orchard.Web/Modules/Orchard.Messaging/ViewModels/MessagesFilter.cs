using Orchard.Messaging.Models;

namespace Orchard.Messaging.ViewModels {
    public class MessagesFilter {
        public QueuedMessageStatus? Status { get; set; }
    }
}