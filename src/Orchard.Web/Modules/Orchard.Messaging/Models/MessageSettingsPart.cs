using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Messaging.Models {
    public class MessageSettingsPart : ContentPart<MessageSettingsPartRecord> {

        [StringLength(MessageSettingsPartRecord.DefaultChannelServiceLength)]
        public string DefaultChannelService {
            get { return Record.DefaultChannelService; }
            set { Record.DefaultChannelService = value;  }
        }
    }
}
