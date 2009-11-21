using System.Web;
using System.Web.Routing;
using Orchard.Blogs.Services;

namespace Orchard.Blogs.Routing {
    public class IsBlogConstraint : IRouteConstraint {
        private readonly IBlogService _blogService;

        public IsBlogConstraint(IBlogService blogService) {
            _blogService = blogService;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            return _blogService.Get(values[parameterName].ToString()) != null;
        }
    }
}