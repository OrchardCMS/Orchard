using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Mvc.Filters;
using Orchard.Mvc.Routes;
using Orchard.Settings;

namespace Orchard.Mvc {
    public class MvcModule : Module {

        protected override void Load(ContainerBuilder moduleBuilder) {
            moduleBuilder.RegisterType<FilterResolvingActionInvoker>().As<IActionInvoker>().InstancePerDependency();
            moduleBuilder.RegisterType<ShellRoute>().InstancePerDependency();

            moduleBuilder.Register(ctx => HttpContextBaseFactory(ctx)).As<HttpContextBase>().InstancePerDependency();
            moduleBuilder.Register(ctx => RequestContextFactory(ctx)).As<RequestContext>().InstancePerDependency();
            moduleBuilder.Register(ctx => UrlHelperFactory(ctx)).As<UrlHelper>().InstancePerDependency();
        }

        private static bool IsRequestValid() {
            if (HttpContext.Current == null)
                return false;

            try {
                // The "Request" property throws at application startup on IIS integrated pipeline mode
                var req = HttpContext.Current.Request;
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        static HttpContextBase HttpContextBaseFactory(IComponentContext context) {
            if (IsRequestValid()) {
                return new HttpContextWrapper(HttpContext.Current);
            }

            // this doesn't work in a background service, throws an exception in ContentManager.Handlers
            //var siteService = context.Resolve<ISiteService>();
            //var baseUrl = siteService.GetSiteSettings().BaseUrl;

            var session = context.Resolve<ISessionLocator>().For(typeof(ContentItem));
            var shellSettings = context.Resolve<ShellSettings>();

            var tableName = "Settings_SiteSettings2PartRecord";
            if (!string.IsNullOrEmpty(shellSettings.DataTablePrefix)) {
                tableName = shellSettings.DataTablePrefix + "_" + tableName;
            }
            var query = session.CreateSQLQuery("SELECT BaseUrl FROM " + tableName);
            var baseUrl = query.UniqueResult<string>();

            return new HttpContextPlaceholder(baseUrl);
        }

        static RequestContext RequestContextFactory(IComponentContext context) {
            var httpContextAccessor = context.Resolve<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.Current();
            if (httpContext != null) {

                var mvcHandler = httpContext.Handler as MvcHandler;
                if (mvcHandler != null) {
                    return mvcHandler.RequestContext;
                }

                var hasRequestContext = httpContext.Handler as IHasRequestContext;
                if (hasRequestContext != null) {
                    if (hasRequestContext.RequestContext != null)
                        return hasRequestContext.RequestContext;
                }
            }
            else {
                httpContext = HttpContextBaseFactory(context);
            }

            return new RequestContext(httpContext, new RouteData());
        }

        static UrlHelper UrlHelperFactory(IComponentContext context) {
            return new UrlHelper(context.Resolve<RequestContext>(), context.Resolve<RouteCollection>());
        }

        /// <summary>
        /// standin context for background tasks.
        /// </summary>
        class HttpContextPlaceholder : HttpContextBase {
            private readonly string _baseUrl;

            public HttpContextPlaceholder(string baseUrl) {
                _baseUrl = baseUrl;
            }

            public override HttpRequestBase Request {
                get { return new HttpRequestPlaceholder(new Uri(_baseUrl)); }
            }

            public override IHttpHandler Handler { get; set; }

            public override HttpResponseBase Response {
                get { return new HttpResponsePlaceholder(); }
            }
        }

        private class HttpResponsePlaceholder : HttpResponseBase {
            public override string ApplyAppPathModifier(string virtualPath) {
                return virtualPath;
            }
        }

        /// <summary>
        /// standin context for background tasks. 
        /// </summary>
        class HttpRequestPlaceholder : HttpRequestBase {
            private readonly Uri _uri;

            public HttpRequestPlaceholder(Uri uri) {
                _uri = uri;
            }

            /// <summary>
            /// anonymous identity provided for background task.
            /// </summary>
            public override bool IsAuthenticated {
                get { return false; }
            }

            // empty collection provided for background operation
            public override NameValueCollection Form {
                get {
                    return new NameValueCollection();
                }
            }

            public override Uri Url {
                get {
                    return _uri;
                }
            }

            public override NameValueCollection Headers {
                get {
                    return new NameValueCollection {{"Host", _uri.Authority}};
                }
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get {
                    return "~/";
                }
            }

            public override string ApplicationPath {
                get {
                    return _uri.LocalPath;
                }
            }

            public override NameValueCollection ServerVariables {
                get {
                    return new NameValueCollection {
                        { "SERVER_PORT", _uri.Port.ToString(CultureInfo.InvariantCulture) },
                        { "HTTP_HOST", _uri.Authority.ToString(CultureInfo.InvariantCulture) },
                        
                    };
                }
            }

        }
    }
}