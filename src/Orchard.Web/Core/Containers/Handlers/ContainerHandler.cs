using System.Web.Routing;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;

namespace Orchard.Core.Containers.Handlers {
    public class ContainerHandler : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.Has(typeof(ContainerPart)))
            {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary
                {
                    {"Area", "Containers"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"Id", context.ContentItem.Id}
                };
            }
        }
    }
}