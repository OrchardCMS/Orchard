using System;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.Modules.Routing {
    public interface IFeatureNameConstraint : IRouteConstraint, ISingletonDependency {
    }

    public class FeatureNameConstraint : IFeatureNameConstraint {
        private readonly IModuleService _moduleService;

        public FeatureNameConstraint(IModuleService moduleService) {
            _moduleService = moduleService;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value))
                return _moduleService.GetModuleByFeatureName(Convert.ToString(value)) != null;

            return false;
        }
    }
}