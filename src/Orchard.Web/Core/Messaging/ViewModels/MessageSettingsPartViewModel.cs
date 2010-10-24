using Orchard.Core.Messaging.Models;
using System.Collections.Generic;

namespace Orchard.Core.Messaging.ViewModels {
    public class MessageSettingsPartViewModel {
        public MessageSettingsPart MessageSettings { get; set; }
        public IEnumerable<string> ChannelServices { get; set; }
    }
}
