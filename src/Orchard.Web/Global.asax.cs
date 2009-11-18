using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Modules;
using Orchard.Environment;

namespace Orchard.Web {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication {
        private static IOrchardHost _host;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = "" } // Parameter defaults
                );
        }

        static IEnumerable<string> OrchardLocationFormats() {
            return new[] {
                "~/Packages/{2}/Views/{1}/{0}.aspx",
                "~/Packages/{2}/Views/{1}/{0}.ascx",
                "~/Packages/{2}/Views/Shared/{0}.aspx",
                "~/Packages/{2}/Views/Shared/{0}.ascx",
                "~/Core/{2}/Views/{1}/{0}.aspx",
                "~/Core/{2}/Views/{1}/{0}.ascx",
                "~/Core/{2}/Views/Shared/{0}.aspx",
                "~/Core/{2}/Views/Shared/{0}.ascx",
            };
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);

            //TEMP: Modules should be able to register their stuff
            var viewEngine = ViewEngines.Engines.OfType<VirtualPathProviderViewEngine>().Single();
            viewEngine.AreaViewLocationFormats = OrchardLocationFormats().Concat(viewEngine.AreaViewLocationFormats).ToArray();
            viewEngine.AreaPartialViewLocationFormats = OrchardLocationFormats().Concat(viewEngine.AreaPartialViewLocationFormats).ToArray();


            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();

            //TODO: what's the failed initialization story - IoC failure in app start can leave you with a zombie appdomain
        }


        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        protected void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ControllerBuilder.Current);
            builder.Register(RouteTable.Routes);
            builder.Register(ModelBinders.Binders);
            builder.Register(ModelMetadataProviders.Current);
            builder.Register(ViewEngines.Engines);
        }
    }
}
