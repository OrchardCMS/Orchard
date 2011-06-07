using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Routes {
    public class RoutePublisher : IRoutePublisher {
        private readonly RouteCollection _routeCollection;
        private readonly ShellSettings _shellSettings;
        private readonly ILifetimeScope _shellLifetimeScope;
        private readonly IRunningShellTable _runningShellTable;

        public RoutePublisher(
            RouteCollection routeCollection,
            ShellSettings shellSettings,
            ILifetimeScope shellLifetimeScope,
            IRunningShellTable runningShellTable) {
            _routeCollection = routeCollection;
            _shellSettings = shellSettings;
            _shellLifetimeScope = shellLifetimeScope;
            _runningShellTable = runningShellTable;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes) {
            var routesArray = routes
                .OrderByDescending(r => r.Route is ServiceRoute ? -1 : 1)
                .ThenByDescending(r => r.Priority)
                .ToArray();

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
                    ShellRoute shellRoute = new ShellRoute(routeDescriptor.Route, _shellSettings, _shellLifetimeScope, _runningShellTable);
                    _routeCollection.Add(routeDescriptor.Name, shellRoute);
                }
            }
        }
    }
}