using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Extensions;
using Autofac.Core;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        private readonly IExtensionManager _extensionManager;

        public MvcModule(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        protected override void Load(ContainerBuilder moduleBuilder) {
            var extensions = _extensionManager.ActiveExtensions();
            var assemblies = extensions.Select(x => x.Assembly);
            var actionInvokerService = new UniqueService();
            moduleBuilder.RegisterType<FilterResolvingActionInvoker>().As(actionInvokerService).InstancePerDependency();

            moduleBuilder.RegisterControllers(new OrchardControllerIdentificationStrategy(extensions), assemblies.ToArray())
                .InjectActionInvoker(actionInvokerService).InstancePerDependency();

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
            var httpContext = context.Resolve<HttpContextBase>();
            var mvcHandler = httpContext.Handler as MvcHandler;
            if (mvcHandler != null) {
                return mvcHandler.RequestContext;
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
        }
    }
}