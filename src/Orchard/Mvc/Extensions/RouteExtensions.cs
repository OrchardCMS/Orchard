using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Mvc.Extensions {
    public static class RouteExtensions {
        public static string GetAreaName(this RouteBase route) {
            if (route is IRouteWithArea routeWithArea) {
                return routeWithArea.Area;
            }

            if (route is Route castRoute && castRoute.DataTokens != null) {
                return castRoute.DataTokens["area"] as string;
            }

            return null;
        }

        public static string GetAreaName(this RouteData routeData) =>
            routeData.DataTokens.TryGetValue("area", out object area) ?
                area as string : GetAreaName(routeData.Route);

        public static string ToRouteString(this RouteValueDictionary route) =>
            string.Join("/", route["Area"], route["Controller"], route["Action"]);
    }
}
