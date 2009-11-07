using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Mvc.RouteProviders {
    public class DefaultRouteProvider : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Name = "Default",
                                                     Priority = -20,
                                                     Route = new Route(
                                                         "{controller}/{action}/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"controller", "home"},
                                                                                      {"action", "index"},
                                                                                      {"id", ""},
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"controller", new HomeOrAccount()}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

        //TEMP: this is hardcoded to allow base web app controllers to pass
        public class HomeOrAccount : IRouteConstraint {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
                object value;
                if (values.TryGetValue(parameterName, out value)) {
                    var parameterValue = Convert.ToString(value);
                    return string.Equals(parameterValue, "home", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(parameterValue, "account", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
        }
    }
}
