using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Autofac;
using Autofac.Core;
using Autofac.Features.Metadata;
using Orchard.WebApi.Extensions;

namespace Orchard.WebApi {
    public class DefaultOrchardWebApiHttpControllerSelector : DefaultHttpControllerSelector, IHttpControllerSelector {
        private readonly HttpConfiguration _configuration;

        public DefaultOrchardWebApiHttpControllerSelector(HttpConfiguration configuration) : base(configuration) {
            _configuration = configuration;
        }

        /// <summary>
        /// Tries to resolve an instance for the controller associated with a given service key for the work context scope.
        /// </summary>
        /// <typeparam name="T">The type of the controller.</typeparam>
        /// <param name="workContext">The work context.</param>
        /// <param name="serviceKey">The service key for the controller.</param>
        /// <param name="instance">The controller instance.</param>
        /// <returns>True if the controller was resolved; false otherwise.</returns>
        protected bool TryResolve<T>(WorkContext workContext, object serviceKey, out T instance) {
            if (workContext != null && serviceKey != null) {
                var key = new KeyedService(serviceKey, typeof(T));
                object value;
                if (workContext.Resolve<ILifetimeScope>().TryResolveService(key, out value)) {
                    instance = (T)value;
                    return true;
                }
            }

            instance = default(T);
            return false;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request) {
            var routeData = request.GetRouteData();
            
            // Determine the area name for the request, and fall back to stock orchard controllers
            var areaName = routeData.GetAreaName();

            var controllerName = base.GetControllerName(request);

            // Service name pattern matches the identification strategy
            var serviceKey = (areaName + "/" + controllerName).ToLowerInvariant();

            var controllerContext = new HttpControllerContext(_configuration, routeData, request);
            
            // Now that the request container is known - try to resolve the controller information
            Meta<Lazy<IHttpController>> info;
            var workContext = controllerContext.GetWorkContext();
            if (TryResolve(workContext, serviceKey, out info)) {
                var type = (Type)info.Metadata["ControllerType"];

                return 
                    new HttpControllerDescriptor(_configuration, controllerName, type);
            }

            return null;
        }
    }
}