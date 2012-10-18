using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Orchard.Core.Navigation.Services {
    public class NavigationManager : INavigationManager {
        private readonly IEnumerable<INavigationProvider> _navigationProviders;
        private readonly IEnumerable<IMenuProvider> _menuProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<INavigationFilter> _navigationFilters;
        private readonly UrlHelper _urlHelper;
        private readonly IOrchardServices _orchardServices;

        public NavigationManager(
            IEnumerable<INavigationProvider> navigationProviders, 
            IEnumerable<IMenuProvider> menuProviders,
            IAuthorizationService authorizationService,
            IEnumerable<INavigationFilter> navigationFilters,
            UrlHelper urlHelper, 
            IOrchardServices orchardServices) {
            _navigationProviders = navigationProviders;
            _menuProviders = menuProviders;
            _authorizationService = authorizationService;
            _navigationFilters = navigationFilters;
            _urlHelper = urlHelper;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            var sources = GetSources(menuName);
            return FinishMenu(Reduce(Merge(sources), menuName == "admin").ToArray());
        }

        public IEnumerable<MenuItem> BuildMenu(IContent menu) {
            var sources = GetSources(menu);
            return FinishMenu(Reduce(Arrange(Filter(Merge(sources))), false).ToArray());
        }

        public string GetNextPosition(IContent menu) {
            var sources = GetSources(menu);
            return Position.GetNext(Reduce(Arrange(Filter(Merge(sources))), false).ToArray());
        }

        public IEnumerable<string> BuildImageSets(string menuName) {
            return GetImageSets(menuName).SelectMany(imageSets => imageSets.Distinct()).Distinct();
        }

        private IEnumerable<MenuItem> FinishMenu(IEnumerable<MenuItem> menuItems) {
            foreach (var menuItem in menuItems) {
                menuItem.Href = GetUrl(menuItem.Url, menuItem.RouteValues);
                menuItem.Items = FinishMenu(menuItem.Items.ToArray());
            }

            return menuItems;
        }

        private IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> menuItems) {
            IEnumerable<MenuItem> result = menuItems;
            foreach(var filter in _navigationFilters) {
                result = filter.Filter(result);
            }

            return result;
        }

        public string GetUrl(string menuItemUrl, RouteValueDictionary routeValueDictionary) {
            var url = string.IsNullOrEmpty(menuItemUrl) && (routeValueDictionary == null || routeValueDictionary.Count == 0)
                          ? "~/"
                          : !string.IsNullOrEmpty(menuItemUrl)
                                ? menuItemUrl
                                : _urlHelper.RouteUrl(routeValueDictionary);

            if (!string.IsNullOrEmpty(url) && _urlHelper.RequestContext.HttpContext != null &&
                !(url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("/"))) {
                if (url.StartsWith("~/")) {
                    url = url.Substring(2);
                }
                var appPath = _urlHelper.RequestContext.HttpContext.Request.ApplicationPath;
                if (appPath == "/")
                    appPath = "";
                url = string.Format("{0}/{1}", appPath, url);
            }
            return url;
        }

        /// <summary>
        /// Updates the items by checking for permissions
        /// </summary>
        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items, bool isAdminMenu) {
            var hasDebugShowAllMenuItems = _authorizationService.TryCheckAccess(Permission.Named("DebugShowAllMenuItems"), _orchardServices.WorkContext.CurrentUser, null);

            foreach (var item in items) {
                if (
                    // debug flag is on
                    hasDebugShowAllMenuItems ||

                    // a content item is linked and the user can view it
                    item.Content != null && item.Permissions.Concat(new[] { Contents.Permissions.ViewContent }).Any(x => _authorizationService.TryCheckAccess(x, _orchardServices.WorkContext.CurrentUser, item.Content)) ||

                    // it's the admin menu and permissions are effective
                    isAdminMenu && (!item.Permissions.Any() || item.Permissions.Any(x => _authorizationService.TryCheckAccess(x, _orchardServices.WorkContext.CurrentUser, null))) ) {

                    yield return new MenuItem {
                        Items = Reduce(item.Items, isAdminMenu),
                        Permissions = item.Permissions,
                        Position = item.Position,
                        RouteValues = item.RouteValues,
                        LocalNav = item.LocalNav,
                        Culture = item.Culture,
                        Text = item.Text,
                        IdHint = item.IdHint,
                        Classes = item.Classes,
                        Url = item.Url,
                        LinkToFirstChild = item.LinkToFirstChild,
                        Href = item.Href,
                        Content = item.Content
                    };
                }
            }
        }

        private IEnumerable<IEnumerable<MenuItem>> GetSources(string menuName) {
            foreach (var provider in _navigationProviders) {
                if (provider.MenuName == menuName) {
                    var builder = new NavigationBuilder();
                    IEnumerable<MenuItem> items = null;
                    try {
                        provider.GetNavigation(builder);
                        items = builder.Build();
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Unexpected error while querying a navigation provider. It was ignored. The menu provided by the provider may not be complete.");
                    }
                    if (items != null) {
                        yield return items;
                    }
                }
            }
        }

        private IEnumerable<IEnumerable<MenuItem>> GetSources(IContent menu) {
            foreach (var provider in _menuProviders) {
                var builder = new NavigationBuilder();
                IEnumerable<MenuItem> items = null;
                try {
                    provider.GetMenu(menu, builder);
                    items = builder.Build();
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Unexpected error while querying a menu provider. It was ignored. The menu provided by the provider may not be complete.");
                }
                if (items != null) {
                    yield return items;
                }
            }
        }

        private IEnumerable<IEnumerable<string>> GetImageSets(string menuName) {
            foreach (var provider in _navigationProviders) {
                if (provider.MenuName == menuName) {
                    var builder = new NavigationBuilder();
                    IEnumerable<string> imageSets = null;
                    try {
                        provider.GetNavigation(builder);
                        imageSets = builder.BuildImageSets();
                    }
                    catch (Exception ex) {
                        Logger.Error(ex, "Unexpected error while querying a navigation provider. It was ignored. The menu provided by the provider may not be complete.");
                    }
                    if (imageSets != null) {
                        yield return imageSets;
                    }
                }
            }
        }

        private static IEnumerable<MenuItem> Merge(IEnumerable<IEnumerable<MenuItem>> sources) {
            var comparer = new MenuItemComparer();
            var orderer = new FlatPositionComparer();

            return sources.SelectMany(x => x).ToArray()
                // group same menus
                .GroupBy(key => key, (key, items) => Join(items), comparer)
                // group same position
                .GroupBy(item => item.Position)
                // order position groups by position
                .OrderBy(positionGroup => positionGroup.Key, orderer)
                // ordered by item text in the postion group
                .SelectMany(positionGroup => positionGroup.OrderBy(item => item.Text == null ? "" : item.Text.TextHint));
        }

        /// <summary>
        /// Organizes a list of <see cref="MenuItem"/> into a hierarchy based on their positions
        /// </summary>
        private static IEnumerable<MenuItem> Arrange(IEnumerable<MenuItem> items) {
            
            var result = new List<MenuItem>();
            var index = new Dictionary<string, MenuItem>();

            foreach (var item in items) {
                MenuItem parent = null;
                var parentPosition = String.Empty;

                var lastSegment = item.Position.LastIndexOf('.');
                if (lastSegment != -1) {
                    parentPosition = item.Position.Substring(0, lastSegment);
                }

                if (index.TryGetValue(parentPosition, out parent)) {
                    parent.Items = parent.Items.Concat(new [] { item });
                }
                else {
                    result.Add(item);
                }

                if (!index.ContainsKey(item.Position)) {
                    // prevent invalid positions
                    index.Add(item.Position, item);    
                }
            }

            return result;
        }

        static MenuItem Join(IEnumerable<MenuItem> items) {
            var list = items.ToArray();

            if (list.Count() < 2)
                return list.Single();

            var joined = new MenuItem {
                Text = list.First().Text,
                IdHint = list.Select(x => x.IdHint).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Classes = list.Select(x => x.Classes).FirstOrDefault(x => x != null && x.Count > 0),
                Url = list.Select(x => x.Url).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Href = list.Select(x => x.Href).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                LinkToFirstChild = list.First().LinkToFirstChild,
                RouteValues = list.Select(x => x.RouteValues).FirstOrDefault(x => x != null),
                LocalNav = list.Any(x => x.LocalNav),
                Culture = list.First().Culture,
                Items = Merge(list.Select(x => x.Items)).ToArray(),
                Position = SelectBestPositionValue(list.Select(x => x.Position)),
                Permissions = list.SelectMany(x => x.Permissions).Distinct(),
                Content = list.First().Content
            };

            return joined;
        }

        private static string SelectBestPositionValue(IEnumerable<string> positions) {
            var comparer = new FlatPositionComparer();
            return positions.Aggregate(string.Empty,
                                       (agg, pos) =>
                                       string.IsNullOrEmpty(agg)
                                           ? pos
                                           : string.IsNullOrEmpty(pos)
                                                 ? agg
                                                 : comparer.Compare(agg, pos) < 0 ? agg : pos);
        }
    }
}