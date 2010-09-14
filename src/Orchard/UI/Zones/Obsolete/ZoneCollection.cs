using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Zones {
#if REFACTORING
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

        public void AddRenderStatic(string location, string templateName, object model) {
            AddZoneItem(location, new RenderStaticZoneItem { Model = model, TemplateName = templateName });
        }

        public void AddDisplayItem(string location, ContentItemViewModel viewModel) {
            AddZoneItem(location, new ContentItemDisplayZoneItem { ViewModel = viewModel });
        }
        public void AddDisplayPart(string location, object model, string templateName, string prefix) {
            AddZoneItem(location, new ContentPartDisplayZoneItem { Model = model, TemplateName = templateName, Prefix = prefix });
        }
        public void AddEditorPart(string location, object model, string templateName, string prefix) {
            AddZoneItem(location, new ContentPartEditorZoneItem { Model = model, TemplateName = templateName, Prefix = prefix });
        }

        public void AddRenderAction(string location, string actionName) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName });
        }
        public void AddRenderAction(string location, string actionName, object routeValues) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName, RouteValues = new RouteValueDictionary(routeValues) });
        }
        public void AddRenderAction(string location, string actionName, RouteValueDictionary routeValues) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName, RouteValues = routeValues });
        }
        public void AddRenderAction(string location, string actionName, string controllerName) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName, ControllerName = controllerName });
        }
        public void AddRenderAction(string location, string actionName, string controllerName, object routeValues) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName, ControllerName = controllerName, RouteValues = new RouteValueDictionary(routeValues) });
        }
        public void AddRenderAction(string location, string actionName, string controllerName, RouteValueDictionary routeValues) {
            AddZoneItem(location, new RenderActionZoneItem { ActionName = actionName, ControllerName = controllerName, RouteValues = routeValues });
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
#endif
}