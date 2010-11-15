using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Routable.Handlers {
    public class RoutePartHandler : ContentHandler {
        private readonly IOrchardServices _services;
        private readonly IRoutablePathConstraint _routablePathConstraint;
        private readonly IRoutableService _routableService;

        public RoutePartHandler(IOrchardServices services, IRepository<RoutePartRecord> repository, IRoutablePathConstraint routablePathConstraint, IRoutableService routableService) {
            _services = services;
            _routablePathConstraint = routablePathConstraint;
            _routableService = routableService;
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            Action<RoutePart> processSlug = (
                routable => {
                    if (!_routableService.ProcessSlug(routable))
                        _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                     routable.Slug, routable.GetEffectiveSlug(), routable.ContentItem.ContentType));
                });

            OnGetDisplayShape<RoutePart>(SetModelProperties);
            OnGetEditorShape<RoutePart>(SetModelProperties);
            OnUpdateEditorShape<RoutePart>(SetModelProperties);

            OnPublished<RoutePart>((context, route) => {
                var path = route.Path;
                route.Path = route.GetPathWithSlug(route.Slug);

                if (context.PublishingItemVersionRecord != null)
                    processSlug(route);

                // if the path has changed by having the slug changed on the way in (e.g. user input) or to avoid conflict
                // then update and publish all contained items
                if (path != route.Path) {
                    _routablePathConstraint.RemovePath(path);
                    _routableService.FixContainedPaths(route);
                }

                if (!string.IsNullOrWhiteSpace(route.Path))
                    _routablePathConstraint.AddPath(route.Path);
            });

            OnRemoved<RoutePart>((context, route) => {
                if (!string.IsNullOrWhiteSpace(route.Path))
                    _routablePathConstraint.RemovePath(route.Path);
            });

            OnIndexing<RoutePart>((context, part) => context.DocumentIndex.Add("title", part.Record.Title).RemoveTags().Analyze());
        }

        private static void SetModelProperties(BuildShapeContext context, RoutePart routable) {
            var item = context.Shape;
            item.Title = routable.Title;
            item.Slug = routable.Slug;
            item.Path = routable.Path;
        }

        public Localizer T { get; set; }
    }

    public class RoutePartHandlerBase : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var routable = context.ContentItem.As<RoutePart>();
            if (routable != null) {
                context.Metadata.DisplayText = routable.Title;
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
