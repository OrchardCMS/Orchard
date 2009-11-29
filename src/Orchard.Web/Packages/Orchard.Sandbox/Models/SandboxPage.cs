using System.Web.Routing;
using Orchard.Models;

namespace Orchard.Sandbox.Models {
    public class SandboxPage : ContentPart<SandboxPageRecord>, IContentDisplayInfo {

        public readonly static ContentType ContentType = new ContentType {Name = "sandboxpage", DisplayName = "Sandbox Page"};

        string IContentDisplayInfo.DisplayText {
            get { return Record.Name; }
        }

        RouteValueDictionary IContentDisplayInfo.DisplayRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Sandbox", controller = "Page", action = "Show", id = ContentItem.Id });
        }

        RouteValueDictionary IContentDisplayInfo.EditRouteValues() {
            return new RouteValueDictionary(new { area = "Orchard.Sandbox", controller = "Page", action = "Edit", id = ContentItem.Id });
        }

    }
}
