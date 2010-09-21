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
                    var originalSlug = routable.Slug;
                    if (!_routableService.ProcessSlug(routable)) {
                        _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                     originalSlug, routable.Slug, routable.ContentItem.ContentType));
                    }

                    // TEMP: path format patterns replaces this logic
                    routable.Path = routable.GetPathWithSlug(routable.Slug);
                });

            OnGetDisplayShape<RoutePart>(SetModelProperties);
            OnGetEditorShape<RoutePart>(SetModelProperties);
            OnUpdateEditorShape<RoutePart>(SetModelProperties);

            OnPublished<RoutePart>((context, routable) => {
                if (context.PublishingItemVersionRecord != null)
                    processSlug(routable);
                if (!string.IsNullOrEmpty(routable.Path))
                    _routablePathConstraint.AddPath(routable.Path);
            });

            OnIndexing<RoutePart>((context, part) => context.DocumentIndex.Add("title", part.Record.Title).RemoveTags().Analyze());
        }

        private static void SetModelProperties(BuildModelContext context, RoutePart routable) {
            var item = context.Model;
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
