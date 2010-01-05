using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.UI.Zones {
    public interface IZoneContainer {
        ZoneCollection Zones { get; }
    }

    public class ZoneCollection : Dictionary<string, ZoneEntry> {
        public void AddAction(string location, Action<HtmlHelper> action) {
            AddZoneItem(location, new DelegateZoneItem { Action = action });
        }
        public void AddRenderPartial(string location, string templateName, object model) {
            AddZoneItem(location, new RenderPartialZoneItem { Model = model, TemplateName = templateName });
        }
        public void AddDisplayItem(string location, ItemDisplayModel displayModel) {
            AddZoneItem(location, new ItemDisplayZoneItem { DisplayModel = displayModel });
        }
        public void AddDisplayPart(string location, object model, string templateName, string prefix) {
            AddZoneItem(location, new PartDisplayZoneItem { Model = model, TemplateName = templateName, Prefix = prefix });
        }
        public void AddEditorPart(string location, object model, string templateName, string prefix) {
            AddZoneItem(location, new PartEditorZoneItem { Model = model, TemplateName = templateName, Prefix = prefix });
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