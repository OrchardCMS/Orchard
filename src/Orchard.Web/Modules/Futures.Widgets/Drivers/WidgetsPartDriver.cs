using System.Web.Routing;
using Futures.Widgets.Models;
using Orchard.ContentManagement.Drivers;

namespace Futures.Widgets.Drivers {
    public class WidgetsPartDriver : ContentItemDriver<WidgetPart> {
        public override RouteValueDictionary GetEditorRouteValues(WidgetPart item) {
            return new RouteValueDictionary {
                {"Area", "Futures.Widgets"},
                {"Controller", "Admin"},
                {"Action", "Edit"},
                {"Id", item.ContentItem.Id}
            };
        }

        protected override bool UseDefaultTemplate {
            get { return true; }
        }
    }
}