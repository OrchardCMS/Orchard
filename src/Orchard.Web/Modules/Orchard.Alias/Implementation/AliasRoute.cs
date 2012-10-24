using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Implementation.Map;

namespace Orchard.Alias.Implementation {
    public class AliasRoute : RouteBase, IRouteWithArea {
        private readonly AliasMap _aliasMap;
        private readonly IRouteHandler _routeHandler;

        public AliasRoute(IAliasHolder aliasHolder, string areaName, IRouteHandler routeHandler) {
            Area = areaName;
            _aliasMap = aliasHolder.GetMap(areaName);
            _routeHandler = routeHandler;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext) {
            // don't compute unnecessary virtual path if the map is empty
            if (!_aliasMap.Any()) {
                return null;
            }

            // Get the full inbound request path
            var virtualPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            // Attempt to lookup RouteValues in the alias map
            IDictionary<string, string> routeValues;
            // TODO: Might as well have the lookup in AliasHolder...
            if (_aliasMap.TryGetAlias(virtualPath, out routeValues)) {
                // Construct RouteData from the route values
                var data = new RouteData(this, _routeHandler);
                foreach (var routeValue in routeValues) {
                    var key = routeValue.Key;
                    if (key.EndsWith("-"))
                        data.Values.Add(key.Substring(0, key.Length - 1), routeValue.Value);
                    else
                        data.Values.Add(key, routeValue.Value);
                }

                data.Values["area"] = Area;
                data.DataTokens["area"] = Area;

                return data;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues) {
            // Lookup best match for route values in the expanded tree
            var match = _aliasMap.Locate(routeValues);
            if (match != null) {
                // Build any "spare" route values onto the Alias (so we correctly support any additional query parameters)
                var sb = new StringBuilder(match.Item2);
                var extra = 0;
                foreach (var routeValue in routeValues) {
                    // Ignore any we already have
                    if (match.Item1.ContainsKey(routeValue.Key)) {
                        continue;
                    }

                    // Add a query string fragment
                    sb.Append((extra++ == 0) ? '?' : '&');
                    sb.Append(Uri.EscapeDataString(routeValue.Key));
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(Convert.ToString(routeValue.Value, CultureInfo.InvariantCulture)));
                }
                // Construct data
                var data = new VirtualPathData(this, sb.ToString());
                // Set the Area for this route
                data.DataTokens["area"] = Area;
                return data;
            }

            return null;
        }

        public string Area { get; private set; }
    }
}