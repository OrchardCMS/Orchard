using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Routing;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Routes {
    public class RoutePublisher : IRoutePublisher {
        private readonly RouteCollection _routeCollection;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRunningShellTable _runningShellTable;

        public RoutePublisher(
            RouteCollection routeCollection,
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IRunningShellTable runningShellTable) {
            _routeCollection = routeCollection;
            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
            _runningShellTable = runningShellTable;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes) {
            var routesArray = routes
                .OrderByDescending(r => r.Priority)
                .ToArray();

            // this is not called often, but is intended to surface problems before
            // the actual collection is modified
            var preloading = new RouteCollection();
            foreach (var routeDescriptor in routesArray) {

                // extract the WebApi route implementation
                var httpRouteDescriptor = routeDescriptor as HttpRouteDescriptor;
                if (httpRouteDescriptor != null) {
                    var httpRouteCollection = new RouteCollection();
                    httpRouteCollection.MapHttpRoute(httpRouteDescriptor.Name, httpRouteDescriptor.RouteTemplate, httpRouteDescriptor.Defaults);
                    routeDescriptor.Route = httpRouteCollection.First();
                }

                preloading.Add(routeDescriptor.Name, routeDescriptor.Route);
            }
                

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
                    var shellRoute = new ShellRoute(routeDescriptor.Route, _shellSettings, _workContextAccessor, _runningShellTable){IsHttpRoute = routeDescriptor is HttpRouteDescriptor};
                    _routeCollection.Add(routeDescriptor.Name, shellRoute);
                }
            }
        }
    }
}