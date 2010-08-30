using Orchard.Core.Messaging.Models;
using Orchard.Mvc.ViewModels;
using System.Collections.Generic;

namespace Orchard.Core.Messaging.ViewModels {
    public class ContentSubscriptionPartViewModel : BaseViewModel {
        public MessageSettingsPart MessageSettings { get; set; }
        public IEnumerable<string> ChannelServices { get; set; }
    }
}
