using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Models.ViewModels;

namespace Orchard.UI.Zones {
    public class ZoneCollection : Dictionary<string, ZoneEntry> {
        public void AddRenderPartial(string location, string templateName, object model) {

        }
        public void AddDisplayItem(string location, ItemDisplayModel displayModel) {

        }
        public void AddAction(string location, Action<HtmlHelper> action) {
            AddZoneItem(location, new DelegateZoneItem { Action = action });
        }

        private void AddZoneItem(string location, ZoneItem item) {
            string zoneName;
            var position = string.Empty;

            var colonIndex = location.IndexOf(':');
            if (colonIndex == -1) {
                zoneName = location.Trim();
            }
            else {
                zoneName = location.Substring(0, colonIndex).Trim();
                position = location.Substring(colonIndex + 1).Trim();
            }

            item.Position = position;
            ZoneEntry entry;
            if (TryGetValue(zoneName, out entry)) {
                entry.Items.Add(item);
            }
            else {
                entry = new ZoneEntry { ZoneName = zoneName, Items = new List<ZoneItem>() };
                Add(zoneName, entry);
                entry.Items.Add(item);
            }

        }
    }
}