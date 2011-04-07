using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.WarmupStarter;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);

            Starter.LaunchStartupThread(MvcSingletons);
        }

        protected void Application_BeginRequest() {
            Starter.OnBeginRequest(Context, MvcSingletons);
        }

        protected void Application_EndRequest() {
            Starter.OnEndRequest();
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }
    }
}
