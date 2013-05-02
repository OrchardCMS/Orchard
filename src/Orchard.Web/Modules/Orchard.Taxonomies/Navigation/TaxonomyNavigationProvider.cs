using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Navigation;

namespace Orchard.Taxonomies.Navigation {
    /// <summary>
    /// Dynamically injects query results as menu items on NavigationQueryMenuItem elements
    /// </summary>
    public class NavigationQueryProvider : INavigationFilter {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public NavigationQueryProvider(
            IContentManager contentManager,
            ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        public IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> items) {

            foreach (var item in items) {
                if (item.Content != null && item.Content.ContentItem.ContentType == "TaxonomyNavigationMenuItem") {
                    // expand query

                    var taxonomyNavigationPart = item.Content.As<TaxonomyNavigationPart>();

                    var rootTerm = _taxonomyService.GetTerm(taxonomyNavigationPart.TermId);

                    var allTerms = rootTerm != null
                                       ? _taxonomyService.GetChildren(rootTerm)
                                       : _taxonomyService.GetTerms(taxonomyNavigationPart.TaxonomyId);

                    var menuPosition = item.Position;
                    foreach (var contentItem in allTerms) {
                        if (contentItem != null) {
                            var part = contentItem;

                            var menuText = _contentManager.GetItemMetadata(part).DisplayText;
                            var routes = _contentManager.GetItemMetadata(part).DisplayRouteValues;

                            var inserted = new MenuItem {
                                Text = new LocalizedString(menuText),
                                IdHint = item.IdHint,
                                Classes = item.Classes,
                                Url = item.Url,
                                Href = item.Href,
                                LinkToFirstChild = false,
                                RouteValues = routes,
                                LocalNav = item.LocalNav,
                                Items = new MenuItem[0],
                                Position = menuPosition + contentItem.Path.TrimEnd('/').Replace("/", "."),
                                Permissions = item.Permissions,
                                Content = part
                            };

                            yield return inserted;
                        }
                    }
                }
                else {
                    yield return item;
                }
            }
        }
    }
}