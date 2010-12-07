using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);

            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();
        }

        protected void Application_BeginRequest() {
            Context.Items["originalHttpContext"] = Context;

            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }

    }
}
