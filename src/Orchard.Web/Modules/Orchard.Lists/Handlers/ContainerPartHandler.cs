using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;

namespace Orchard.Lists.Handlers {
    public class ContainerPartHandler : ContentHandler {
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var container = context.ContentItem.As<ContainerPart>();

            if (container == null)
                return;

            // Containers link to their contents in admin screens.
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Lists"},
                {"Controller", "Admin"},
                {"Action", "List"},
                {"containerId", container.Id}
            };
        }
    }
}