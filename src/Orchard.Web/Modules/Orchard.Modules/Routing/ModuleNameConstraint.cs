using System;
using System.Web;
using System.Web.Routing;

namespace Orchard.Modules.Routing {
    public interface IModuleNameConstraint : IRouteConstraint, ISingletonDependency {
    }

    public class ModuleNameConstraint : IModuleNameConstraint {
        private readonly IModuleService _moduleService;

        public ModuleNameConstraint(IModuleService moduleService) {
            _moduleService = moduleService;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value))
                return _moduleService.GetModuleByName(Convert.ToString(value)) != null;

            return false;
        }
    }
}