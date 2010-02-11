using System.Collections.Generic;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.UI.Zones;

namespace Orchard.Mvc.ViewModels {
    public class BaseViewModel : IZoneContainer {
        public BaseViewModel() {
            Messages = new List<NotifyEntry>();
            Zones = new ZoneCollection();
        }

        public IList<NotifyEntry> Messages { get; set; }
        public IUser CurrentUser { get; set; }
        public ZoneCollection Zones { get; set; }

        public IEnumerable<MenuItem> Menu { get; set; }
    }
}
