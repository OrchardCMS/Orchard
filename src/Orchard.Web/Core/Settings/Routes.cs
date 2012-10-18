using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Core.Settings.Controllers;
using Orchard.Mvc.Routes;

namespace Orchard.Core.Settings {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Settings/{groupInfoId}",
                        new RouteValueDictionary {
                            {"area", "Settings"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            {"groupInfoId",  new SettingsActionConstraint()}
                        },
                        new RouteValueDictionary {
                            {"area", "Settings"},
                            {"groupInfoId", ""}
                        },
                        new MvcRouteHandler())
                }
            };
        }

   }

    public class SettingsActionConstraint : IRouteConstraint {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            if (!values.ContainsKey(parameterName))
                return false;
            
            // just hard-coding to know action name strings for now
            var potentialActionName = values[parameterName] as string;
            return !string.IsNullOrWhiteSpace(potentialActionName)
                   && !potentialActionName.Equals("Index", StringComparison.OrdinalIgnoreCase)
                   && !potentialActionName.Equals("Culture", StringComparison.OrdinalIgnoreCase)
                   && !potentialActionName.Equals("AddCulture", StringComparison.OrdinalIgnoreCase)
                   && !potentialActionName.Equals("DeleteCulture", StringComparison.OrdinalIgnoreCase)
                ;
        }
    }
}