using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.UI.Navigation;

namespace Orchard.Projections.Navigation {
    /// <summary>
    /// Dynamically injects query results as menu items on NavigationQueryMenuItem elements
    /// </summary>
    public class NavigationQueryProvider : INavigationFilter {
        private readonly IContentManager _contentManager;
        private readonly IProjectionManager _projectionManager;

        public NavigationQueryProvider(
            IContentManager contentManager,
            IProjectionManager projectionManager) {
            _contentManager = contentManager;
            _projectionManager = projectionManager;
        }

        public IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> items) {

            foreach (var item in items) {
                if (item.Content != null && item.Content.ContentItem.ContentType == "NavigationQueryMenuItem") {
                    // expand query
                    var navigationQuery = item.Content.As<NavigationQueryPart>();
                    var contentItems = _projectionManager.GetContentItems(navigationQuery.QueryPartRecord.Id, navigationQuery.Skip, navigationQuery.Items).ToList();

                    var menuPosition = item.Position;
                    int index = 0;
                    foreach (var contentItem in contentItems) {
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
                                Position = menuPosition + ":" + index++,
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