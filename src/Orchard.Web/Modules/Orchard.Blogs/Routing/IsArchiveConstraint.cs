using System.Web;
using System.Web.Routing;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Routing {
    public class IsArchiveConstraint : IRouteConstraint {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
                          RouteDirection routeDirection) {
            return values[parameterName] != null && (new ArchiveData(values[parameterName].ToString())).Year > 0;
        }
    }
}