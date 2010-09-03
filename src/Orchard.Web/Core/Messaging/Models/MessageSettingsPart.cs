using Orchard.ContentManagement;

namespace Orchard.Core.Messaging.Models {
    public class MessageSettingsPart : ContentPart<MessageSettingsPartRecord> {
        public string DefaultChannelService {
            get { return Record.DefaultChannelService; }
            set { Record.DefaultChannelService = value;  }
        }
    
    }
}
