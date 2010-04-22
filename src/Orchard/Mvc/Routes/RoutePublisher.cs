using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Routes {
    public class RoutePublisher : IRoutePublisher {
        private readonly RouteCollection _routeCollection;
        private readonly ShellSettings _shellSettings;
        private readonly Func<RouteBase, ShellRoute> _shellRouteFactory;

        public RoutePublisher(
            RouteCollection routeCollection,
            ShellSettings shellSettings,
            Func<RouteBase, ShellRoute> shellRouteFactory) {
            _routeCollection = routeCollection;
            _shellSettings = shellSettings;
            _shellRouteFactory = shellRouteFactory;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes) {
            var routesArray = routes.OrderByDescending(r => r.Priority).ToArray();

            // this is not called often, but is intended to surface problems before
            // the actual collection is modified
            var preloading = new RouteCollection();
            foreach (var route in routesArray)
                preloading.Add(route.Name, route.Route);

            using (_routeCollection.GetWriteLock()) {
                // existing routes are removed while the collection is briefly inaccessable
                var cropArray = _routeCollection
                    .OfType<ShellRoute>()
                    .Where(sr => sr.ShellSettingsName == _shellSettings.Name)
                    .ToArray();

                foreach(var crop in cropArray) {
                    _routeCollection.Remove(crop);
                }

                // new routes are added
                foreach (var routeDescriptor in routesArray) {
                    //_routeCollection.Add(route.Name, _shellRouteFactory(_shellSettings.Name, route.Route));
                    _routeCollection.Add(routeDescriptor.Name, _shellRouteFactory(routeDescriptor.Route));
                }
            }
        }
    }
}