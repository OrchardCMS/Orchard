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
    public class DefaultOrchardWebApiHttpHttpControllerActivator : IHttpControllerActivator {
        private readonly HttpConfiguration _configuration;

        public DefaultOrchardWebApiHttpHttpControllerActivator(HttpConfiguration configuration) : base() {
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
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType) {
            var routeData = request.GetRouteData();

            HttpControllerContext controllerContext = new HttpControllerContext(_configuration, routeData, request);
            
            // Determine the area name for the request, and fall back to stock orchard controllers
            var areaName = routeData.GetAreaName();

            // Service name pattern matches the identification strategy
            var serviceKey = (areaName + "/" + controllerDescriptor.ControllerName).ToLowerInvariant();

            // Now that the request container is known - try to resolve the controller information
            Meta<Lazy<IHttpController>> info;
            var workContext = controllerContext.GetWorkContext();
            if (TryResolve(workContext, serviceKey, out info)) {
                controllerContext.ControllerDescriptor =
                    new HttpControllerDescriptor(_configuration, controllerDescriptor.ControllerName, controllerType);

                var controller = info.Value.Value;

                controllerContext.Controller = controller;

                return controller;
            }

            return null;
        }
    }
}