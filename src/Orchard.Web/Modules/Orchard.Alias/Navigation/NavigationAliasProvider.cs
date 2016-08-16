using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Navigation;

namespace Orchard.Alias.Navigation {

    [OrchardFeature("Orchard.Alias.BreadcrumbLink")]
    public class NavigationAliasProvider : INavigationFilter {
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _hca;

        public NavigationAliasProvider(
            IAliasService aliasService,
            IContentManager contentManager,
            IHttpContextAccessor hca) {
            _aliasService = aliasService;
            _contentManager = contentManager;
            _hca = hca;
        }

        public IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> items) {
            foreach (var item in items) {
                if (item.Content != null && item.Content.ContentItem.ContentType == "AliasBreadcrumbMenuItem") {
                    var request = _hca.Current().Request;
                    var path = request.Path;
                    var appPath = request.ApplicationPath ?? "/";
                    var requestUrl = (path.StartsWith(appPath) ? path.Substring(appPath.Length) : path).TrimStart('/');
                    var endsWithSlash = requestUrl.EndsWith("/");

                    var menuPosition = item.Position;

                    var urlLevel = endsWithSlash ? requestUrl.Count(l => l == '/') - 1 : requestUrl.Count(l => l == '/');
                    var menuLevel = menuPosition.Count(l => l == '.');

                    // Checking the menu item whether it's the leaf element or it's an intermediate element
                    // or it's an unneccessary element according to the url.
                    RouteValueDictionary contentRoute;
                    if (menuLevel == urlLevel) {
                        contentRoute = request.RequestContext.RouteData.Values;
                    }
                    else {
                        // If menuLevel doesn't equal to urlLevel it can mean that this menu item is
                        // an intermediate element (the difference is a positive value) or this menu
                        // item is lower in the navigation hierarchy according to the url (negative
                        // value). If the value is negative, removing the menu item, if the value
                        // is positive finding its place in the hierarchy.
                        var levelDifference = urlLevel - menuLevel;
                        if (levelDifference > 0) {
                            if (endsWithSlash) {
                                levelDifference += levelDifference;
                            }
                            for (int i = 0; i < levelDifference; i++) {
                                requestUrl = requestUrl.Remove(requestUrl.LastIndexOf('/'));
                                path = path.Remove(path.LastIndexOf('/'));
                            }
                            contentRoute = _aliasService.Get(requestUrl);
                            if (contentRoute == null) {
                                // After the exact number of segments is cut out from the url and the
                                // currentRoute is still null, trying another check with the added slash,
                                // because we don't know if the alias was created with a slash at the end or not.
                                contentRoute = _aliasService.Get(requestUrl.Insert(requestUrl.Length, "/"));
                                path = path.Insert(path.Length, "/");
                                if (contentRoute == null) {
                                    contentRoute = new RouteValueDictionary();
                                }
                            }
                        }
                        else {
                            contentRoute = new RouteValueDictionary();
                        }
                    }

                    object id;
                    contentRoute.TryGetValue("Id", out id);
                    int contentId;
                    int.TryParse(id as string, out contentId);
                    if (contentId == 0) {
                        // If failed to get the Id's value from currentRoute, transform the alias to the virtual path
                        // and try to get the content item's id from there. E.g. "Blogs/Blog/Item?blogId=12" where
                        // the last digits represents the content item's id. If there is a match in the path we get
                        // the digits after the equality sign.
                        // There is an another type of the routes: like "Contents/Item/Display/13", but when the
                        // content item's route is in this form we already have the id from contentRoute.TryGetValue("Id", out id).
                        var virtualPath = _aliasService.LookupVirtualPaths(contentRoute, _hca.Current()).FirstOrDefault();
                        int.TryParse(virtualPath != null ? virtualPath.VirtualPath.Substring(virtualPath.VirtualPath.LastIndexOf('=') + 1) : "0", out contentId);
                    }
                    if (contentId != 0) {
                        var currentContentItem = _contentManager.Get(contentId);
                        if (currentContentItem != null) {
                            var menuText = _contentManager.GetItemMetadata(currentContentItem).DisplayText;
                            var routes = _contentManager.GetItemMetadata(currentContentItem).DisplayRouteValues;

                            var inserted = new MenuItem {
                                Text = new LocalizedString(menuText),
                                IdHint = item.IdHint,
                                Classes = item.Classes,
                                Url = path,
                                Href = item.Href,
                                LinkToFirstChild = false,
                                RouteValues = routes,
                                LocalNav = item.LocalNav,
                                Items = Enumerable.Empty<MenuItem>(),
                                Position = menuPosition,
                                Permissions = item.Permissions,
                                Content = item.Content
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