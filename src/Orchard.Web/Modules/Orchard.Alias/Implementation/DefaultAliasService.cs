using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Implementation.Storage;
using Orchard.Mvc.Routes;
using Orchard.Utility.Extensions;

namespace Orchard.Alias.Implementation {
    public class DefaultAliasService : IAliasService {
        private readonly IAliasStorage _aliasStorage;
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly Lazy<IEnumerable<RouteDescriptor>> _routeDescriptors;
        private readonly IAliasHolder _aliasHolder;

        public DefaultAliasService(
            IAliasStorage aliasStorage,
            IEnumerable<IRouteProvider> routeProviders,
            IAliasHolder aliasHolder) {
            _aliasStorage = aliasStorage;
            _routeProviders = routeProviders;
            _aliasHolder = aliasHolder;

            _routeDescriptors = new Lazy<IEnumerable<RouteDescriptor>>(GetRouteDescriptors);
        }

        public RouteValueDictionary Get(string aliasPath) {
            return _aliasStorage.Get(aliasPath).ToRouteValueDictionary();
        }

        public void Set(string aliasPath, RouteValueDictionary routeValues, string aliasSource, bool isManaged) {
            _aliasStorage.Set(
                aliasPath,
                ToDictionary(routeValues),
                aliasSource,
                isManaged);
        }

        public void Set(string aliasPath, string routePath, string aliasSource, bool isManaged) {
            _aliasStorage.Set(
                aliasPath.TrimStart('/'),
                ToDictionary(routePath),
                aliasSource,
                isManaged);
        }

        public void Delete(string aliasPath) {

            if (aliasPath == null) {
                aliasPath = String.Empty;
            }

            _aliasStorage.Remove(aliasPath);
        }

        public void Delete(string aliasPath, string aliasSource) {

            if (aliasPath == null) {
                aliasPath = String.Empty;
            }

            _aliasStorage.Remove(aliasPath, aliasSource);
        }

        public void DeleteBySource(string aliasSource) {
            _aliasStorage.RemoveBySource(aliasSource);
        }

        public IEnumerable<string> Lookup(string routePath) {
            return Lookup(ToDictionary(routePath).ToRouteValueDictionary());
        }

        public void Replace(string aliasPath, RouteValueDictionary routeValues, string aliasSource, bool isManaged) {
            foreach (var lookup in Lookup(routeValues).Where(path => path != aliasPath)) {
                Delete(lookup, aliasSource);
            }
            Set(aliasPath, routeValues, aliasSource, isManaged);
        }

        public void Replace(string aliasPath, string routePath, string aliasSource, bool isManaged) {
            Replace(aliasPath, ToDictionary(routePath).ToRouteValueDictionary(), aliasSource, isManaged);
        }

        public IEnumerable<string> Lookup(RouteValueDictionary routeValues) {
            object area;

            if (routeValues.TryGetValue("area", out area)) {
                // the route has an area, lookup in the specific alias map

                var map = _aliasHolder.GetMap(area.ToString());

                if (map == null) {
                    return Enumerable.Empty<string>();
                }

                var locate = map.Locate(routeValues);

                if (locate == null) {
                    return Enumerable.Empty<string>();
                }

                return new[] { locate.Item2 };
            }

            // no specific area, lookup in all alias maps
            var result = new List<string>();
            foreach (var map in _aliasHolder.GetMaps()) {
                var locate = map.Locate(routeValues);

                if (locate != null) {
                    result.Add(locate.Item2);
                }
            }

            return result;
        }

        public IEnumerable<Tuple<string, RouteValueDictionary>> List() {
            return _aliasStorage.List().Select(item => Tuple.Create(item.Item1, item.Item3.ToRouteValueDictionary()));
        }

        public IEnumerable<Tuple<string, RouteValueDictionary, string>> List(string sourceStartsWith) {
            return _aliasStorage.List(sourceStartsWith).Select(item => Tuple.Create(item.Item1, item.Item3.ToRouteValueDictionary(), item.Item4));
        }

        public IEnumerable<VirtualPathData> LookupVirtualPaths(RouteValueDictionary routeValues, HttpContextBase httpContext) {
            return Utils.LookupVirtualPaths(httpContext, _routeDescriptors.Value, routeValues);
        }

        private IDictionary<string, string> ToDictionary(string routePath) {
            if (routePath == null)
                return null;

            return Utils.LookupRouteValues(new StubHttpContext(), _routeDescriptors.Value, routePath);
        }

        private static IDictionary<string, string> ToDictionary(IEnumerable<KeyValuePair<string, object>> routeValues) {
            if (routeValues == null)
                return null;

            return routeValues.ToDictionary(kv => kv.Key, kv => Convert.ToString(kv.Value, CultureInfo.InvariantCulture));
        }

        private IEnumerable<RouteDescriptor> GetRouteDescriptors() {
            return _routeProviders
                .SelectMany(routeProvider => {
                    var routes = new List<RouteDescriptor>();
                    routeProvider.GetRoutes(routes);
                    return routes;
                })
                .Where(routeDescriptor => !(routeDescriptor.Route is AliasRoute))
                .OrderByDescending(routeDescriptor => routeDescriptor.Priority);
        }

        private class StubHttpContext : HttpContextBase {
            public override HttpRequestBase Request {
                get { return new StubHttpRequest(); }
            }

            private class StubHttpRequest : HttpRequestBase { }
        }
    }
}