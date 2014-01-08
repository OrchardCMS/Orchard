using Orchard.ContentManagement;

namespace Orchard.Messaging.Models {
    public class MessageSettingsPart : ContentPart {

        public MessageQueueStatus Status {
            get { return this.Retrieve(x => x.Status); }
            set { this.Store(x => x.Status, value); }
        }
    }
}
