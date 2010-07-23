using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Data;

namespace Orchard.Core.Routable.Handlers {
    public class RoutePartHandler : ContentHandler {
        private readonly IRoutablePathConstraint _routablePathConstraint;

        public RoutePartHandler(IRepository<RoutePartRecord> repository, IRoutablePathConstraint routablePathConstraint) {
            _routablePathConstraint = routablePathConstraint;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<RoutePart>((context, routable) => {
                if (!string.IsNullOrEmpty(routable.Path))
                    _routablePathConstraint.AddPath(routable.Path);
            });
        }
    }
    public class RoutePartHandlerBase : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var routable = context.ContentItem.As<RoutePart>();
            if (routable != null) {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Routable"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"path", routable.Path}
                };
            }
        }
    }
}
