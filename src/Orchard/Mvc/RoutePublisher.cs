using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Autofac.Integration.Web;

namespace Orchard.Mvc {
    public class RoutePublisher : IRoutePublisher {
        private readonly RouteCollection _routeCollection;
        private readonly IContainerProvider _containerProvider;

        public RoutePublisher(RouteCollection routeCollection, IContainerProvider containerProvider) {
            _routeCollection = routeCollection;
            _containerProvider = containerProvider;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes) {
            var routesArray = routes.OrderByDescending(r=>r.Priority).ToArray();

            // this is not called often, but is intended to surface problems before
            // the actual collection is modified
            var preloading = new RouteCollection();
            foreach (var route in routesArray)
                preloading.Add(route.Name, route.Route);

            using (_routeCollection.GetWriteLock()) {
                // existing routes are removed while the collection is briefly inaccessable
                _routeCollection.Clear();

                // new routes are added
                foreach (var route in routesArray) {
                    _routeCollection.Add(route.Name, route.Route);
                }

                // and painted with the IContainerProvider if it's type has data tokens
                foreach (var route in routesArray.Select(d => d.Route).OfType<Route>()) {
                    if (route.DataTokens == null)
                        route.DataTokens = new RouteValueDictionary();
                    route.DataTokens["IContainerProvider"] = _containerProvider;
                }
            }
        }
    }
}
