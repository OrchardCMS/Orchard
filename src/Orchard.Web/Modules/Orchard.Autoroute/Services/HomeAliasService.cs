using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Alias;
using Orchard.Alias.Implementation.Holder;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {
    public class HomeAliasService : IHomeAliasService {
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;
        private readonly IAliasHolder _aliasHolder;
        private const string AliasSource = "Autoroute:Home";
        private const string HomeAlias = "";

        private RouteValueDictionary _homeAliasRoute;

        public HomeAliasService(IAliasService aliasService, IAliasHolder aliasHolder, IContentManager contentManager) {
            _aliasService = aliasService;
            _aliasHolder = aliasHolder;
            _contentManager = contentManager;
        }

        public RouteValueDictionary GetHomeRoute() {
            if(_homeAliasRoute == null) {
                _homeAliasRoute = _aliasService.Get(HomeAlias);
            }

            return _homeAliasRoute;
        }

        public int? GetHomePageId(VersionOptions version = null) {
            var homePage = GetHomePage(version);
            return homePage != null ? homePage.Id : default(int?);
        }

        public IContent GetHomePage(VersionOptions version = null) {
            var homePageRoute = GetHomeRoute();
            
            if (homePageRoute == null)
                return null;
            
            var alias = LookupAlias(homePageRoute);

            if (alias == null)
                return null;

            var homePage = _contentManager.Query<AutoroutePart, AutoroutePartRecord>(version).Where(x => x.DisplayAlias == alias).Slice(0, 1).FirstOrDefault();
            return homePage;
        }

        public bool IsHomePage(IContent content, VersionOptions homePageVersion = null) {
            var homePageId = GetHomePageId(homePageVersion);
            return content.Id == homePageId;
        }

        public void PublishHomeAlias(IContent content) {
            var routeValues = _contentManager.GetItemMetadata(content).DisplayRouteValues;
            PublishHomeAlias(routeValues);
        }

        public void PublishHomeAlias(string route) {
            _aliasService.DeleteBySource(AliasSource);
            _aliasService.Set(HomeAlias, route, AliasSource);
        }

        public void PublishHomeAlias(RouteValueDictionary route) {
            _homeAliasRoute = route;
            _aliasService.DeleteBySource(AliasSource);
            _aliasService.Set(HomeAlias, route, AliasSource);
        }

        private string LookupAlias(RouteValueDictionary routeValues) {
            object area;

            if (!routeValues.TryGetValue("area", out area))
                return null;

            var map = _aliasHolder.GetMap(area.ToString());
            if (map == null)
                return null;

            var alias = map.GetAliases().FirstOrDefault(x => IsSameRoute(x.RouteValues, routeValues) && !String.IsNullOrWhiteSpace(x.Path));
            return alias != null ? alias.Path : null;
        }

        private bool IsSameRoute(IDictionary<string,string> a, RouteValueDictionary b) {
            if(a.Count != b.Count) {
                return false;
            }

            return a.Keys.All(x => String.Equals(a[x], Convert.ToString(b[x]), StringComparison.OrdinalIgnoreCase));
        }
    }
}
