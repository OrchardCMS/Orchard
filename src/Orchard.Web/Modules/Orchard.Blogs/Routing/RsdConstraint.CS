using Orchard.Alias.Implementation.Holder;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Orchard.Blogs.Routing {
    public class RsdConstraint : IRsdConstraint {
        private readonly IAliasHolder _aliasHolder;

        public RsdConstraint(IAliasHolder aliasHolder) {
            _aliasHolder = aliasHolder;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                var path = FindPath(parameterValue);
                if(path == null) {
                    return false;
                }

                IDictionary<string, string> routeValues;
                if (!_aliasHolder.GetMap("Orchard.Blogs").TryGetAlias(path, out routeValues)) {
                    return false;
                }

                var isBlog =
                    //routeValues.ContainsKey("area") &&
                    //routeValues["area"] == "Orchard.Blogs" &&
                    routeValues.ContainsKey("controller") &&
                    routeValues["controller"] == "Blog" &&
                    routeValues.ContainsKey("action") &&
                    routeValues["action"] == "Item"
                    ;

                return isBlog;
            }

            return false;
        }

        public string FindPath(string path) {
            if (path.EndsWith("/rsd", StringComparison.OrdinalIgnoreCase)) {
                return path.Substring(0, path.Length - "/rsd".Length);
            }

            // blog is on homepage
            if(path.Equals("rsd", StringComparison.OrdinalIgnoreCase)) {
                return String.Empty;
            }

            return null;
        }
    }
}