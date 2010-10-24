using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Mvc.Filters;
using Orchard.Mvc.Routes;

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

            return new HttpContextPlaceholder();
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
                httpContext = new HttpContextPlaceholder();
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
            public override HttpRequestBase Request {
                get { return new HttpRequestPlaceholder(); }
            }

            public override IHttpHandler Handler { get; set; }
        }

        /// <summary>
        /// standin context for background tasks. 
        /// </summary>
        class HttpRequestPlaceholder : HttpRequestBase {
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
        }
    }
}