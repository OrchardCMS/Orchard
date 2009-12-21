using System.Web;
using System.Web.Routing;
using Autofac.Integration.Web;
using Orchard.Blogs.Services;

namespace Orchard.Blogs.Routing {
    public class IsBlogConstraint : IRouteConstraint {
        private readonly IContainerProvider _containerProvider;

        public IsBlogConstraint(IContainerProvider containerProvider) {
            _containerProvider = containerProvider;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            return _containerProvider.RequestContainer.Resolve<IBlogService>().Get(values[parameterName].ToString()) != null;
        }
    }
}