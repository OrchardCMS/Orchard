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
        private UrlPrefix _urlPrefix;

        public ShellRoute(RouteBase route, ShellSettings shellSettings, ILifetimeScope shellLifetimeScope, IRunningShellTable runningShellTable) {
            _route = route;
            _shellSettings = shellSettings;
            _runningShellTable = runningShellTable;
            _container = new LifetimeScopeContainer(shellLifetimeScope);
            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);

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

            var effectiveHttpContext = httpContext;
            if (_urlPrefix != null)
                effectiveHttpContext = new UrlPrefixAdjustedHttpContext(httpContext, _urlPrefix);

            var routeData = _route.GetRouteData(effectiveHttpContext);
            if (routeData == null)
                return null;

            // otherwise paint wrap handler and return it
            var containerProvider = new ContainerProvider(_container);
            routeData.RouteHandler = new RouteHandler(containerProvider, routeData.RouteHandler);
            routeData.DataTokens["IContainerProvider"] = containerProvider;
            return routeData;
        }


        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            // locate appropriate shell settings for request
            var settings = _runningShellTable.Match(requestContext.HttpContext);

            // only proceed if there was a match, and it was for this client
            if (settings == null || settings.Name != _shellSettings.Name)
                return null;

            var effectiveRequestContext = requestContext;
            if (_urlPrefix != null)
                effectiveRequestContext = new RequestContext(new UrlPrefixAdjustedHttpContext(requestContext.HttpContext, _urlPrefix), requestContext.RouteData);

            var virtualPath = _route.GetVirtualPath(effectiveRequestContext, values);
            if (virtualPath == null)
                return null;

            if (_urlPrefix != null)
                virtualPath.VirtualPath = _urlPrefix.PrependLeadingSegments(virtualPath.VirtualPath);

            return virtualPath;
        }

        class ContainerProvider : IContainerProvider {
            public ContainerProvider(IContainer applicationContainer) {
                ApplicationContainer = applicationContainer;
            }

            public void BeginRequestLifetime() {
                RequestLifetime = ApplicationContainer.BeginLifetimeScope("httpRequest");
            }

            public void EndRequestLifetime() {
                RequestLifetime.Dispose();
                RequestLifetime = null;
            }

            public IContainer ApplicationContainer { get; set; }
            public ILifetimeScope RequestLifetime { get; set; }
        }

        class RouteHandler : IRouteHandler {
            private readonly ContainerProvider _containerProvider;
            private readonly IRouteHandler _routeHandler;

            public RouteHandler(ContainerProvider containerProvider, IRouteHandler routeHandler) {
                _containerProvider = containerProvider;
                _routeHandler = routeHandler;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext) {
                var httpHandler = _routeHandler.GetHttpHandler(requestContext);
                if (httpHandler is IHttpAsyncHandler) {
                    return new HttpAsyncHandler(_containerProvider, (IHttpAsyncHandler)httpHandler);
                }
                return new HttpHandler(_containerProvider, httpHandler);
            }
        }

        class HttpHandler : IHttpHandler, IRequiresSessionState, IHasRequestContext {
            protected readonly ContainerProvider _containerProvider;
            private readonly IHttpHandler _httpHandler;

            public HttpHandler(ContainerProvider containerProvider, IHttpHandler httpHandler) {
                _containerProvider = containerProvider;
                _httpHandler = httpHandler;
            }

            public bool IsReusable {
                get { return false; }
            }

            public void ProcessRequest(HttpContext context) {
                _containerProvider.BeginRequestLifetime();
                try {
                    _httpHandler.ProcessRequest(context);
                }
                finally {
                    _containerProvider.EndRequestLifetime();
                }
            }

            public RequestContext RequestContext {
                get {
                    var mvcHandler = _httpHandler as MvcHandler;
                    return mvcHandler == null ? null : mvcHandler.RequestContext;
                }
            }
        }

        class HttpAsyncHandler : HttpHandler, IHttpAsyncHandler {
            private readonly IHttpAsyncHandler _httpAsyncHandler;

            public HttpAsyncHandler(ContainerProvider containerProvider, IHttpAsyncHandler httpAsyncHandler)
                : base(containerProvider, httpAsyncHandler) {
                _httpAsyncHandler = httpAsyncHandler;
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
                _containerProvider.BeginRequestLifetime();
                try {
                    return _httpAsyncHandler.BeginProcessRequest(context, cb, extraData);
                }
                catch {
                    _containerProvider.EndRequestLifetime();
                    throw;
                }
            }

            public void EndProcessRequest(IAsyncResult result) {
                try {
                    _httpAsyncHandler.EndProcessRequest(result);
                }
                finally {
                    _containerProvider.EndRequestLifetime();
                }
            }
        }
    }
}
