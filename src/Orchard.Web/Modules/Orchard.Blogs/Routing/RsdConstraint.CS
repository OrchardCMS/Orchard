using System;
using System.Web;
using System.Web.Routing;

namespace Orchard.Blogs.Routing {
    public class RsdConstraint : IRsdConstraint {
        private readonly IBlogPathConstraint _blogPathConstraint;

        public RsdConstraint(IBlogPathConstraint blogPathConstraint) {
            _blogPathConstraint = blogPathConstraint;
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

                return _blogPathConstraint.FindPath(path) != null;
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