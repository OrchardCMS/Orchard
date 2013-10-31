using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Messaging.Models {
    public class MessageSettingsPart : ContentPart {

        public const ushort DefaultChannelServiceLength = 64;

        [StringLength(DefaultChannelServiceLength)]
        public string DefaultChannelService {
            get { return this.Retrieve(x => x.DefaultChannelService); }
            set { this.Store(x => x.DefaultChannelService, value);  }
        }
    }
}
