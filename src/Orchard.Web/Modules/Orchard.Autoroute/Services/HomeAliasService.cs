using System;
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

        public HomeAliasService(IAliasService aliasService, IAliasHolder aliasHolder, IContentManager contentManager) {
            _aliasService = aliasService;
            _aliasHolder = aliasHolder;
            _contentManager = contentManager;
        }

        public RouteValueDictionary GetHomeRoute() {
            return _aliasService.Get(HomeAlias);
        }

        public int? GetHomePageId(VersionOptions version = null) {
            var homePage = GetHomePage(version);
            return homePage != null ? homePage.Id : default(int?);
        }

        public IContent GetHomePage(VersionOptions version = null) {
            var homePageRoute = GetHomeRoute();
            var alias = LookupAlias(homePageRoute);

            if (alias == null)
                return null;

            var homePage = _contentManager.Query<AutoroutePart, AutoroutePartRecord>(version).Where(x => x.DisplayAlias == alias).Slice(0, 1).SingleOrDefault();
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

            var alias = map.GetAliases().FirstOrDefault(x => !String.IsNullOrWhiteSpace(x.Path));
            return alias != null ? alias.Path : null;
        }
    }
}