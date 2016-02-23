using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI;
using Orchard.UI.Navigation;
using Orchard.Utility;
using Orchard.Exceptions;

namespace Orchard.Core.Navigation.Services {
    public class NavigationManager : INavigationManager {
        private readonly IEnumerable<INavigationProvider> _navigationProviders;
        private readonly IEnumerable<IMenuProvider> _menuProviders;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<INavigationFilter> _navigationFilters;
        private readonly UrlHelper _urlHelper;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;

        public NavigationManager(
            IEnumerable<INavigationProvider> navigationProviders, 
            IEnumerable<IMenuProvider> menuProviders,
            IAuthorizationService authorizationService,
            IEnumerable<INavigationFilter> navigationFilters,
            UrlHelper urlHelper, 
            IOrchardServices orchardServices,
            ShellSettings shellSettings) {
            _navigationProviders = navigationProviders;
            _menuProviders = menuProviders;
            _authorizationService = authorizationService;
            _navigationFilters = navigationFilters;
            _urlHelper = urlHelper;
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            var sources = GetSources(menuName);
            var hasDebugShowAllMenuItems = _authorizationService.TryCheckAccess(Permission.Named("DebugShowAllMenuItems"), _orchardServices.WorkContext.CurrentUser, null);
            return FinishMenu(Reduce(Filter(Merge(sources)), menuName == "admin", hasDebugShowAllMenuItems).ToArray());
        }

        public IEnumerable<MenuItem> BuildMenu(IContent menu) {
            var sources = GetSources(menu);
            var hasDebugShowAllMenuItems = _authorizationService.TryCheckAccess(Permission.Named("DebugShowAllMenuItems"), _orchardServices.WorkContext.CurrentUser, null);
            return FinishMenu(Reduce(Arrange(Filter(Merge(sources))), false, hasDebugShowAllMenuItems).ToArray());
        }

        public string GetNextPosition(IContent menu) {
            var sources = GetSources(menu);
            var hasDebugShowAllMenuItems = _authorizationService.TryCheckAccess(Permission.Named("DebugShowAllMenuItems"), _orchardServices.WorkContext.CurrentUser, null);
            return Position.GetNext(Reduce(Arrange(Filter(Merge(sources))), false, hasDebugShowAllMenuItems).ToArray());
        }

        public IEnumerable<string> BuildImageSets(string menuName) {
            return GetImageSets(menuName).SelectMany(imageSets => imageSets.Distinct()).Distinct();
        }

        private IEnumerable<MenuItem> FinishMenu(ICollection<MenuItem> menuItems) {
            foreach (var menuItem in menuItems) {
                menuItem.Href = GetUrl(menuItem.Url, menuItem.RouteValues);
                menuItem.Items = FinishMenu(menuItem.Items.ToArray());
            }

            return menuItems;
        }

        private IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> menuItems) {
            IEnumerable<MenuItem> result = menuItems;
            foreach (var filter in _navigationFilters) {
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

            var schemes = new[] { "http", "https", "tel", "mailto" };
            if (!string.IsNullOrEmpty(url) && _urlHelper.RequestContext.HttpContext != null &&
                !(url.StartsWith("/") || schemes.Any(scheme => url.StartsWith(scheme + ":")))) {
                if (!url.StartsWith("#")) {
                    if (url.StartsWith("~/")) {
                        if (!String.IsNullOrEmpty(_shellSettings.RequestUrlPrefix)) {
                            url = _shellSettings.RequestUrlPrefix + "/" + url.Substring(2);
                        }
                        else {
                            url = url.Substring(2);
                        }
                    }
                    var appPath = _urlHelper.RequestContext.HttpContext.Request.ApplicationPath;
                    if (appPath == "/")
                        appPath = "";
                    url = string.Format("{0}/{1}", appPath, url);
                }
            }
            return url;
        }

        /// <summary>
        /// Updates the items by checking for permissions
        /// </summary>
        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items, bool isAdminMenu, bool hasDebugShowAllMenuItems) {
            foreach (var item in items.Where(item => 
                // debug flag is on
                hasDebugShowAllMenuItems ||
                // or item does not have any permissions set
                !item.Permissions.Any() ||
                // or user has permission (either based on the linked item or global, if there's no linked item)
                item.Permissions.Any(x => _authorizationService.TryCheckAccess(
                    x, 
                    _orchardServices.WorkContext.CurrentUser, 
                    item.Content == null || isAdminMenu ? null : item.Content)))) {
                var oldItems = item.Items;

                item.Items = Reduce(item.Items, isAdminMenu, hasDebugShowAllMenuItems).ToList();

                // if all sub items have been filtered out, ensure the main one is not one of them
                // e.g., Manage Roles and Manage Users are not granted, the Users item should not show up 
                if (oldItems.Any() && !item.Items.Any()) {
                    if (oldItems.Any(x => NavigationHelper.RouteMatches(x.RouteValues, item.RouteValues))) {
                        continue;
                    }
                }

                // if there are sub items returns the current item, otherwise ensure this item has valid permissions
                yield return item;
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
                        if (ex.IsFatal()) {
                            throw;
                        } 
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
                    if (ex.IsFatal()) {
                        throw;
                    } 
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
                        if (ex.IsFatal()) {
                            throw;
                        } 
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
                MenuItem parent;
                var parentPosition = String.Empty;

                var position = item.Position ?? String.Empty;

                var lastSegment = position.LastIndexOf('.');
                if (lastSegment != -1) {
                    parentPosition = position.Substring(0, lastSegment);
                }

                if (index.TryGetValue(parentPosition, out parent)) {
                    parent.Items = parent.Items.Concat(new [] { item });
                }
                else {
                    result.Add(item);
                }

                if (!index.ContainsKey(position)) {
                    // prevent invalid positions
                    index.Add(position, item);    
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