using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.Web;
using Orchard.Environment;
using Orchard.Environment.AutofacUtil;
using Orchard.Environment.Configuration;

namespace Orchard.Mvc.Routes {

    public class ShellRoute : RouteBase, IRouteWithArea {
        private readonly RouteBase _route;
        private readonly ShellSettings _shellSettings;
        private readonly IContainer _container;
        private readonly IRunningShellTable _runningShellTable;

        public ShellRoute(RouteBase route, ShellSettings shellSettings, ILifetimeScope shellLifetimeScope, IRunningShellTable runningShellTable) {
            _route = route;
            _shellSettings = shellSettings;
            _runningShellTable = runningShellTable;
            _container = new LifetimeScopeContainer(shellLifetimeScope);

            var routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null) {
                Area = routeWithArea.Area;
            }

            var routeWithDataTokens = route as Route;
            if ((routeWithDataTokens != null) && (routeWithDataTokens.DataTokens != null)) {
                Area = (routeWithDataTokens.DataTokens["area"] as string);
            }
        }

        public string ShellSettingsName { get { return _shellSettings.Name; } }
        public string Area { get; private set; }

        public override RouteData GetRouteData(HttpContextBase httpContext) {
            // locate appropriate shell settings for request
            var settings = _runningShellTable.Match(httpContext);

            // only proceed if there was a match, and it was for this client
            if (settings == null || settings.Name != _shellSettings.Name)
                return null;

            // route didn't match anyway
            var routeData = _route.GetRouteData(httpContext);
            if (routeData == null)
                return null;

            // otherwise paint wrap handler and return it
            routeData.RouteHandler = new RouteHandler(_container, routeData.RouteHandler);
            return routeData;
        }


        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            // todo - ignore conditionally

            var virtualPath = _route.GetVirtualPath(requestContext, values);
            if (virtualPath == null)
                return null;

            return virtualPath;
        }


        class RouteHandler : IRouteHandler {
            private readonly IContainer _container;
            private readonly IRouteHandler _routeHandler;

            public RouteHandler(IContainer container, IRouteHandler routeHandler) {
                _container = container;
                _routeHandler = routeHandler;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext) {
                var httpHandler = _routeHandler.GetHttpHandler(requestContext);
                return new HttpAsyncHandler(
                    _container,
                    requestContext,
                    (IHttpAsyncHandler)httpHandler);
            }
        }

        class HttpAsyncHandler : IHttpAsyncHandler, IRequiresSessionState, IContainerProvider {
            private readonly RequestContext _requestContext;
            private readonly IHttpAsyncHandler _httpAsyncHandler;

            public HttpAsyncHandler(IContainer applicationContainer, RequestContext requestContext, IHttpAsyncHandler httpAsyncHandler) {
                ApplicationContainer = applicationContainer;
                _requestContext = requestContext;
                _httpAsyncHandler = httpAsyncHandler;
            }

            public bool IsReusable {
                get { return false; }
            }

            public void ProcessRequest(HttpContext context) {
                BeginRequestLifetime();
                try {
                    _httpAsyncHandler.ProcessRequest(context);
                }
                finally {
                    EndRequestLifetime();
                }
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
                BeginRequestLifetime();
                try {
                    return _httpAsyncHandler.BeginProcessRequest(context, cb, extraData);
                }
                catch {
                    EndRequestLifetime();
                    throw;
                }
            }

            public void EndProcessRequest(IAsyncResult result) {
                try {
                    _httpAsyncHandler.EndProcessRequest(result);
                }
                finally {
                    EndRequestLifetime();
                }
            }

            public void BeginRequestLifetime() {
                RequestLifetime = ApplicationContainer.BeginLifetimeScope("httpRequest");
                _requestContext.RouteData.DataTokens["IContainerProvider"] = this;
            }

            public void EndRequestLifetime() {
                _requestContext.RouteData.DataTokens.Remove("IContainerProvider");
                RequestLifetime.Dispose();
            }

            public IContainer ApplicationContainer { get; set; }
            public ILifetimeScope RequestLifetime { get; set; }
        }
    }
}
