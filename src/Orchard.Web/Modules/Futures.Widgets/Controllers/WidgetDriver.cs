using System.Web.Routing;
using Futures.Widgets.Models;
using Orchard.ContentManagement.Drivers;

namespace Futures.Widgets.Controllers {
    public class WidgetDriver : ContentItemDriver<Widget> {
        protected override RouteValueDictionary GetEditorRouteValues(Widget item) {
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