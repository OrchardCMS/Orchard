using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Handlers {
    public class RoutableHandler : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var routable = context.ContentItem.As<IsRoutable>();
            if (routable != null) {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Routable"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"Path", context.ContentItem.As<IsRoutable>().Record.Path}
                };
            }
        }
    }
}
