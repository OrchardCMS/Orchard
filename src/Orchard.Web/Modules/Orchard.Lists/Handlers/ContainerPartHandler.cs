using System.Collections.Generic;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Lists.Handlers {
    public class ContainerPartHandler : ContentHandler {
        public ContainerPartHandler() {
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var container = context.ContentItem.As<ContainerPart>();

            if (container == null)
                return;

            // containers link to their contents in admin screens
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Lists"},
                {"Controller", "Admin"},
                {"Action", "List"},
                {"containerId", container.Id}
            };
        }
    }
}