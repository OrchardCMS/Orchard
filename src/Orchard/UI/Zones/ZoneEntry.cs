using System.Collections.Generic;

namespace Orchard.UI.Zones {
    public class ZoneEntry {
        public ZoneEntry() {
            Items = new List<ZoneItem>();
        }

        public string ZoneName { get; set; }
        public IList<ZoneItem> Items { get; set; }
    }
}