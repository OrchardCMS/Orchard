using System.Collections.Generic;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.ViewModels {
    public class MessageSettingsPartViewModel {
        public MessageSettingsPart MessageSettings { get; set; }
        public IEnumerable<string> ChannelServices { get; set; }
    }
}
