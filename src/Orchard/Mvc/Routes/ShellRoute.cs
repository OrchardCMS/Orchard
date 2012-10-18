using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Mvc.Extensions;

namespace Orchard.Mvc.Routes {

    public class ShellRoute : RouteBase, IRouteWithArea {
        private readonly RouteBase _route;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRunningShellTable _runningShellTable;
        private readonly UrlPrefix _urlPrefix;

        public ShellRoute(RouteBase route, ShellSettings shellSettings, IWorkContextAccessor workContextAccessor, IRunningShellTable runningShellTable) {
            _route = route;
            _shellSettings = shellSettings;
            _runningShellTable = runningShellTable;
            _workContextAccessor = workContextAccessor;
            if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                _urlPrefix = new UrlPrefix(_shellSettings.RequestUrlPrefix);

            Area = route.GetAreaName();
        }

        public SessionStateBehavior SessionState { get; set; }

        public string ShellSettingsName { get { return _shellSettings.Name; } }
        public string Area { get; private set; }
        public bool IsHttpRoute { get; set; }

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

            // push provided session state behavior to underlying MvcHandler
            effectiveHttpContext.SetSessionStateBehavior(SessionState);

            // otherwise wrap handler and return it
            routeData.RouteHandler = new RouteHandler(_workContextAccessor, routeData.RouteHandler, SessionState);
            routeData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;

            if (IsHttpRoute) {
                routeData.Values["IWorkContextAccessor"] = _workContextAccessor; // for WebApi
            }
            
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

        class RouteHandler : IRouteHandler {
            private readonly IWorkContextAccessor _workContextAccessor;
            private readonly IRouteHandler _routeHandler;
            private readonly SessionStateBehavior _sessionStateBehavior;

            public RouteHandler(IWorkContextAccessor workContextAccessor, IRouteHandler routeHandler, SessionStateBehavior sessionStateBehavior) {
                _workContextAccessor = workContextAccessor;
                _routeHandler = routeHandler;
                _sessionStateBehavior = sessionStateBehavior;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext) {
                var httpHandler = _routeHandler.GetHttpHandler(requestContext);
                requestContext.HttpContext.SetSessionStateBehavior(_sessionStateBehavior);
                 
                if (httpHandler is IHttpAsyncHandler) {
                    return new HttpAsyncHandler(_workContextAccessor, (IHttpAsyncHandler)httpHandler);
                }
                return new HttpHandler(_workContextAccessor, httpHandler);
            }
        }

        class HttpHandler : IHttpHandler, IRequiresSessionState, IHasRequestContext {
            protected readonly IWorkContextAccessor _workContextAccessor;
            private readonly IHttpHandler _httpHandler;

            public HttpHandler(IWorkContextAccessor workContextAccessor, IHttpHandler httpHandler) {
                _workContextAccessor = workContextAccessor;
                _httpHandler = httpHandler;
            }

            public bool IsReusable {
                get { return false; }
            }

            public void ProcessRequest(HttpContext context) {
                using (_workContextAccessor.CreateWorkContextScope(new HttpContextWrapper(context))) {
                    _httpHandler.ProcessRequest(context);
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
            private IDisposable _scope;

            public HttpAsyncHandler(IWorkContextAccessor containerProvider, IHttpAsyncHandler httpAsyncHandler)
                : base(containerProvider, httpAsyncHandler) {
                _httpAsyncHandler = httpAsyncHandler;
            }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
                _scope = _workContextAccessor.CreateWorkContextScope(new HttpContextWrapper(context));
                try {
                    return _httpAsyncHandler.BeginProcessRequest(context, cb, extraData);
                }
                catch {
                    _scope.Dispose();
                    throw;
                }
            }

            [DebuggerStepThrough]
            public void EndProcessRequest(IAsyncResult result) {
                try {
                    _httpAsyncHandler.EndProcessRequest(result);
                }
                finally {
                    _scope.Dispose();
                }
            }
        }
    }
}
