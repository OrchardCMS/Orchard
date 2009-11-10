using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.CmsPages.Services;
using Orchard.Mvc.Routes;

namespace Orchard.CmsPages {
    public class Routes : IRouteProvider, IRouteConstraint {
        private readonly IPageManager _pageManager;

        public Routes(IPageManager pageManager) {
            _pageManager = pageManager;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            IRouteConstraint slugConstraint = this;
            return new[] {
                             new RouteDescriptor {
                                                     Priority = 10,
                                                     Route = new Route(
                                                         "{*slug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"},
                                                                                      {"controller", "templates"},
                                                                                      {"action", "show"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"slug", slugConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.CmsPages"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            //TEMP: direct db call... 
            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);
                bool result = _pageManager.GetCurrentlyPublishedSlugs().Count(slug => slug == parameterValue) != 0;
                return result;
            }
            return false;
        }
    }
}
