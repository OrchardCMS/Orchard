using System.Web.Mvc;
using System.Web.Routing;
using Orchard.SecureSocketsLayer.Models;

namespace Orchard.SecureSocketsLayer.Services {
    public interface ISecureSocketsLayerService : IDependency {
        bool ShouldBeSecure(string actionName, string controllerName, RouteValueDictionary routeValues);
        bool ShouldBeSecure(RequestContext requestContext);
        bool ShouldBeSecure(ActionExecutingContext actionContext);
        string InsecureActionUrl(string actionName, string controllerName);
        string InsecureActionUrl(string actionName, string controllerName, object routeValues);
        string InsecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues);
        string SecureActionUrl(string actionName, string controllerName);
        string SecureActionUrl(string actionName, string controllerName, object routeValues);
        string SecureActionUrl(string actionName, string controllerName, RouteValueDictionary routeValues);
        SslSettings GetSettings();

    }
}