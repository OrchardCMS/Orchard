using System.Web.Routing;
using Orchard.Models;

namespace Orchard.Sandbox.Models {
    public class SandboxPage : ContentPart<SandboxPageRecord>, IContentDisplayInfo {

        string IContentDisplayInfo.DisplayText {
            get { return Record.Name; }
        }

        RouteValueDictionary IContentDisplayInfo.DisplayRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Wikis", controller = "Page", action = "Show", id = ContentItem.Id });
        }

        RouteValueDictionary IContentDisplayInfo.EditRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Wikis", controller = "Page", action = "Edit", id = ContentItem.Id });
        }

    }
}