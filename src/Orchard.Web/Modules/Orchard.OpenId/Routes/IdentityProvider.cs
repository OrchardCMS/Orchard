using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Orchard.OpenId.Routes {
    [OrchardFeature("Orchard.IdentityProvider")]
    public class IdentityProviderRoutes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            throw new NotImplementedException();
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var route in GetRoutes()) {
                routes.Add(route);
            }
        }
    }
}