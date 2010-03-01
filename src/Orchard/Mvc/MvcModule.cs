using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Extensions;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        private readonly IExtensionManager _extensionManager;

        public MvcModule(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        protected override void Load(ContainerBuilder moduleBuilder) {
            var extensions = _extensionManager.ActiveExtensions();
            var assemblies = extensions.Select(x => x.Assembly);

            var module = new AutofacControllerModule(assemblies.ToArray()) {
                ActionInvokerType = typeof(FilterResolvingActionInvoker),
                IdentificationStrategy = new OrchardControllerIdentificationStrategy(extensions)
            };

            moduleBuilder.RegisterModule(module);
            moduleBuilder.Register(ctx => HttpContextBaseFactory(ctx)).As<HttpContextBase>().FactoryScoped();
            moduleBuilder.Register(ctx => RequestContextFactory(ctx)).As<RequestContext>().FactoryScoped();
            moduleBuilder.Register(ctx => UrlHelperFactory(ctx)).As<UrlHelper>().FactoryScoped();
        }

        static HttpContextBase HttpContextBaseFactory(IContext context) {
            if (HttpContext.Current != null) {
                return new HttpContextWrapper(HttpContext.Current);
            }

            return new HttpContextPlaceholder();
        }

        static RequestContext RequestContextFactory(IContext context) {
            var httpContext = context.Resolve<HttpContextBase>();
            var mvcHandler = httpContext.Handler as MvcHandler;
            if (mvcHandler != null) {
                return mvcHandler.RequestContext;
            }

            return new RequestContext(httpContext, new RouteData());
        }

        static UrlHelper UrlHelperFactory(IContext context) {
            return new UrlHelper(context.Resolve<RequestContext>(), context.Resolve<RouteCollection>());
        }

        /// <summary>
        /// standin context for background tasks.
        /// </summary>
        class HttpContextPlaceholder : HttpContextBase {
            public override HttpRequestBase Request {
                get { return new HttpRequestPlaceholder(); }
            }
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