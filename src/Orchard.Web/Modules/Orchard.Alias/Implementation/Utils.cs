using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Routes;
using Orchard.Mvc.Wrappers;

namespace Orchard.Alias.Implementation {
    public static class Utils {
        public static IDictionary<string, string> LookupRouteValues
            (HttpContextBase httpContext, IEnumerable<RouteDescriptor>
            routeDescriptors,
            string routePath) {
            var queryStringIndex = routePath.IndexOf('?');
            var routePathNoQueryString = queryStringIndex == -1 ? routePath : routePath.Substring(0, queryStringIndex);
            var queryString = queryStringIndex == -1 ? null : routePath.Substring(queryStringIndex + 1);

            var lookupContext = new LookupHttpContext(httpContext, routePathNoQueryString);
            var matches = routeDescriptors
                .Select(routeDescriptor => routeDescriptor.Route.GetRouteData(lookupContext))
                .Where(routeData => routeData != null)
                .Select(data => ToRouteValues(data, queryString));

            return matches.FirstOrDefault();
        }

        public static IEnumerable<VirtualPathData> LookupVirtualPaths(
            HttpContextBase httpContext,
            IEnumerable<RouteDescriptor> routeDescriptors,
            RouteValueDictionary routeValues) {

            var areaName = "";
            object value;
            if (routeValues.TryGetValue("area", out value))
                areaName = Convert.ToString(value, CultureInfo.InvariantCulture);

            var virtualPathDatas = routeDescriptors.Where(r2 => r2.Route.GetAreaName() == areaName)
                .Select(r2 => r2.Route.GetVirtualPath(httpContext.Request.RequestContext, routeValues))
                .Where(vp => vp != null)
                .ToArray();

            return virtualPathDatas;
        }

        public static IEnumerable<VirtualPathData> LookupVirtualPaths(
            HttpContextBase httpContext,
            IEnumerable<RouteDescriptor> routeDescriptors,
            string areaName,
            IDictionary<string, string> routeValues) {

            var routeValueDictionary = new RouteValueDictionary(routeValues.ToDictionary(kv => RemoveDash(kv.Key), kv => (object)kv.Value));
            var virtualPathDatas = routeDescriptors.Where(r2 => r2.Route.GetAreaName() == areaName)
                .Select(r2 => r2.Route.GetVirtualPath(httpContext.Request.RequestContext, routeValueDictionary))
                .Where(vp => vp != null)
                .ToArray();

            return virtualPathDatas;
        }

        private static string RemoveDash(string key) {
            return key.EndsWith("-", StringComparison.InvariantCulture) ? key.Substring(0, key.Length - 1) : key;
        }


        private static Dictionary<string, string> ToRouteValues(RouteData routeData, string queryString) {
            var routeValues = routeData.Values
                .Select(kv => {
                    var value = Convert.ToString(kv.Value, CultureInfo.InvariantCulture);
                    var defaultValue = FindDefault(routeData.Route, kv.Key);
                    if (defaultValue != null && string.Equals(defaultValue, value, StringComparison.InvariantCultureIgnoreCase)) {
                        return new { Key = kv.Key + "-", Value = value };
                    }
                    return new { kv.Key, Value = value };
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            if (queryString != null) {
                foreach (var term in queryString
                    .Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ParseTerm)) {
                    if (!routeValues.ContainsKey(term[0]))
                        routeValues[term[0]] = term[1];
                }
            }
            return routeValues;
        }

        private static string[] ParseTerm(string term) {
            var equalsIndex = term.IndexOf('=');
            if (equalsIndex == -1) {
                return new[] { Uri.UnescapeDataString(term), null };
            }
            return new[] { Uri.UnescapeDataString(term.Substring(0, equalsIndex)), Uri.UnescapeDataString(term.Substring(equalsIndex + 1)) };
        }

        private static string FindDefault(RouteBase route, string key) {
            var route2 = route as Route;
            if (route2 == null) {
                return null;
            }

            object defaultValue;
            if (!route2.Defaults.TryGetValue(key, out defaultValue)) {
                return null;
            }
            return Convert.ToString(defaultValue, CultureInfo.InvariantCulture);
        }

        public class LookupHttpContext : HttpContextBaseWrapper {
            private readonly string _path;

            public LookupHttpContext(HttpContextBase httpContext, string path)
                : base(httpContext) {
                _path = path;
            }

            public override HttpRequestBase Request {
                get { return new LookupHttpRequest(this, base.Request, _path); }
            }

            private class LookupHttpRequest : HttpRequestBaseWrapper {
                private readonly string _path;

                public LookupHttpRequest(HttpContextBase httpContextBase, HttpRequestBase httpRequestBase, string path)
                    : base( /*httpContextBase,*/ httpRequestBase) {
                    _path = path;
                }


                public override string AppRelativeCurrentExecutionFilePath {
                    get { return "~/" + _path; }
                }

                public override string ApplicationPath {
                    get { return "/"; }
                }

                public override string Path {
                    get { return "/" + _path; }
                }

                public override string PathInfo {
                    get { return ""; }
                }
            }
        }
    }
}