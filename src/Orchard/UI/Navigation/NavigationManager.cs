using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
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
        }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            var sources = GetSources(menuName);
            return FinishMenu(Crop(Reduce(Merge(sources))).ToArray());
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

        private static IEnumerable<MenuItem> Crop(IEnumerable<MenuItem> items) {
            return items.Where(item => item.Items.Any() || item.RouteValues != null);
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
                        TextHint = item.TextHint,
                        IdHint = item.IdHint,
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
                    provider.GetNavigation(builder);
                    yield return builder.Build();
                }
            }
        }

        private IEnumerable<IEnumerable<string>> GetImageSets(string menuName) {
            foreach (var provider in _providers) {
                if (provider.MenuName == menuName) {
                    var builder = new NavigationBuilder();
                    provider.GetNavigation(builder);
                    yield return builder.BuildImageSets();
                }
            }
        }

        private static IEnumerable<MenuItem> Merge(IEnumerable<IEnumerable<MenuItem>> sources) {
            var comparer = new MenuItemComparer();
            var orderer = new FlatPositionComparer();

            return sources.SelectMany(x => x).ToArray()
                .GroupBy(key => key, (key, items) => Join(items), comparer)
                .OrderBy(item => item.Position, orderer);
        }

        static MenuItem Join(IEnumerable<MenuItem> items) {
            if (items.Count() < 2)
                return items.Single();

            var joined = new MenuItem {
                Text = items.First().Text,
                TextHint = items.First().TextHint,
                IdHint = items.First().IdHint,
                Url = items.First().Url,
                Href = items.First().Href,
                LinkToFirstChild = items.First().LinkToFirstChild,
                RouteValues = items.First().RouteValues,
                LocalNav = items.Any(x => x.LocalNav),
                Items = Merge(items.Select(x => x.Items)).ToArray(),
                Position = SelectBestPositionValue(items.Select(x => x.Position)),
                Permissions = items.SelectMany(x => x.Permissions)
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