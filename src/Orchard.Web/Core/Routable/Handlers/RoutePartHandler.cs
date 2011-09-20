using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Notify;

namespace Orchard.Core.Routable.Handlers {
    public class RoutePartHandler : ContentHandler {
        private readonly IOrchardServices _services;
        private readonly IRoutablePathConstraint _routablePathConstraint;
        private readonly IRoutableService _routableService;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHomePageProvider _routableHomePageProvider;

        public RoutePartHandler(
            IOrchardServices services,
            IRepository<RoutePartRecord> repository,
            IRoutablePathConstraint routablePathConstraint,
            IRoutableService routableService,
            IContentManager contentManager,
            IWorkContextAccessor workContextAccessor,
            IEnumerable<IHomePageProvider> homePageProviders) {
            _services = services;
            _routablePathConstraint = routablePathConstraint;
            _routableService = routableService;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            Action<RoutePart> processSlug = (
                routable => {
                    if (!_routableService.ProcessSlug(routable))
                        _services.Notifier.Warning(T("Permalinks in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                     routable.Slug, routable.GetEffectiveSlug(), routable.ContentItem.ContentType));
                });

            OnGetDisplayShape<RoutePart>(SetModelProperties);
            OnGetEditorShape<RoutePart>(SetModelProperties);
            OnUpdateEditorShape<RoutePart>(SetModelProperties);

            Action<PublishContentContext, RoutePart> handler = (context, route) => {
                FinalizePath(route, context, processSlug);

                if (_routableHomePageProvider == null)
                    return;

                var homePageSetting = _workContextAccessor.GetContext().CurrentSite.HomePage;
                var currentHomePageId = !string.IsNullOrWhiteSpace(homePageSetting)
                                            ? _routableHomePageProvider.GetHomePageId(homePageSetting)
                                            : 0;

                if (route.Id != 0 && (route.Id == currentHomePageId || route.PromoteToHomePage)) {

                    if (currentHomePageId != route.Id) {
                        // reset the path on the current home page
                        var currentHomePage = _contentManager.Get(currentHomePageId);
                        if (currentHomePage != null)
                            FinalizePath(currentHomePage.As<RoutePart>(), context, processSlug);
                        // set the new home page
                        _services.WorkContext.CurrentSite.HomePage = _routableHomePageProvider.GetSettingValue(route.ContentItem.Id);
                    }

                    // readjust the constraints of the current current home page
                    _routablePathConstraint.RemovePath(route.Path);
                    route.Path = "";
                    _routableService.FixContainedPaths(route);
                    _routablePathConstraint.AddPath(route.Path);
                }
            };

            OnPublished<RoutePart>(handler);
            OnUnpublished<RoutePart>(handler);

            OnRemoved<RoutePart>((context, route) => {
                if (!string.IsNullOrWhiteSpace(route.Path))
                    _routablePathConstraint.RemovePath(route.Path);
            });

            OnIndexing<RoutePart>((context, part) => context.DocumentIndex.Add("title", part.Record.Title).RemoveTags().Analyze());
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<RoutePart>();

            if (part != null) {
                context.Metadata.Identity.Add("Route.Slug", part.Slug);
            }
        }

        private void FinalizePath(RoutePart route, PublishContentContext context, Action<RoutePart> processSlug) {
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
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHomePageProvider _routableHomePageProvider;

        public RoutePartHandlerBase(IWorkContextAccessor workContextAccessor, IEnumerable<IHomePageProvider> homePageProviders) {
            _workContextAccessor = workContextAccessor;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
        }

        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            var routable = context.ContentItem.As<RoutePart>();

            if (routable == null)
                return;

            // set the display route values if it hasn't been set or only has been set by the Contents module. 
            // allows other modules to set their own display. probably not common enough to warrant some priority implementation
            if (context.Metadata.DisplayRouteValues == null || context.Metadata.DisplayRouteValues["Area"] as string == "Contents") {
                var itemPath = routable.Id == _routableHomePageProvider.GetHomePageId(_workContextAccessor.GetContext().CurrentSite.HomePage)
                    ? ""
                    : routable.Path;

                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Routable"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"path", itemPath}
                };
            }
        }
    }
}
