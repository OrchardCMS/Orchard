using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.UI.Navigation {
    public class NavigationManager : INavigationManager {
        private readonly IEnumerable<INavigationProvider> _providers;
        private readonly IAuthorizationService _authorizationService;
        private readonly UrlHelper _urlHelper;
        private readonly IOrchardServices _orchardServices;

        public NavigationManager(IEnumerable<INavigationProvider> providers, IAuthorizationService authorizationService, UrlHelper urlHelper, IOrchardServices orchardServices) {
            _providers = providers;
            _authorizationService = authorizationService;
            _urlHelper = urlHelper;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            var sources = GetSources(menuName);
            return FinishMenu(Reduce(Merge(sources)).ToArray());
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


        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items) {
            var hasDebugShowAllMenuItems = _authorizationService.TryCheckAccess(Permission.Named("DebugShowAllMenuItems"), _orchardServices.WorkContext.CurrentUser, null);
            foreach (var item in items) {
                if (hasDebugShowAllMenuItems ||
                    !item.Permissions.Any() ||
                    item.Permissions.Any(x => _authorizationService.TryCheckAccess(x, _orchardServices.WorkContext.CurrentUser, null))) {
                    yield return new MenuItem {
                        Items = Reduce(item.Items),
                        Permissions = item.Permissions,
                        Position = item.Position,
                        RouteValues = item.RouteValues,
                        LocalNav = item.LocalNav,
                        Text = item.Text,
                        IdHint = item.IdHint,
                        Classes = item.Classes,
                        Url = item.Url,
                        LinkToFirstChild = item.LinkToFirstChild,
                        Href = item.Href
                    };
                }
            }
        }

        private IEnumerable<IEnumerable<MenuItem>> GetSources(string menuName) {
            foreach (var provider in _providers) {
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

        private IEnumerable<IEnumerable<string>> GetImageSets(string menuName) {
            foreach (var provider in _providers) {
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

        static MenuItem Join(IEnumerable<MenuItem> items) {
            if (items.Count() < 2)
                return items.Single();

            var joined = new MenuItem {
                Text = items.First().Text,
                IdHint = items.Select(x => x.IdHint).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Classes = items.Select(x => x.Classes).FirstOrDefault(x => x != null && x.Count > 0),
                Url = items.Select(x => x.Url).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Href = items.Select(x => x.Href).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                LinkToFirstChild = items.First().LinkToFirstChild,
                RouteValues = items.Select(x => x.RouteValues).FirstOrDefault(x => x != null),
                LocalNav = items.Any(x => x.LocalNav),
                Items = Merge(items.Select(x => x.Items)).ToArray(),
                Position = SelectBestPositionValue(items.Select(x => x.Position)),
                Permissions = items.SelectMany(x => x.Permissions).Distinct(),
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