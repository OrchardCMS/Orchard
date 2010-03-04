using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Orchard.UI.Zones {
    public class RenderActionZoneItem : ZoneItem {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }

        public override void Execute<TModel>(HtmlHelper<TModel> html) {
            html.RenderAction(ActionName, ControllerName, RouteValues);
        }
    }
}
