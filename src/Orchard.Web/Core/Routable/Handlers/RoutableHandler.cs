using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Data;

namespace Orchard.Core.Routable.Handlers {
    public class RoutableHandler : ContentHandler {
        private readonly IRoutablePathConstraint _routablePathConstraint;

        public RoutableHandler(IRepository<RoutableRecord> repository, IRoutablePathConstraint routablePathConstraint) {
            _routablePathConstraint = routablePathConstraint;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<IsRoutable>((context, routable) => {
                if (!string.IsNullOrEmpty(routable.Path))
                    _routablePathConstraint.AddPath(routable.Path);
            });
        }
    }
    public class IsRoutableHandler : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var routable = context.ContentItem.As<IsRoutable>();
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
