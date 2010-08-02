using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Orchard.Specs.Hosting.Orchard.Web {
    public class MvcApplication : HttpApplication {
        private static IContainer _hostContainer;
        private static IOrchardHost _host;

        protected void Application_Start() {
            _hostContainer = OrchardStarter.CreateHostContainer(MvcSingletons);
            _host = _hostContainer.Resolve<IOrchardHost>();
            Host.Initialize();

            var route = RouteTable.Routes.MapRoute("foo", "hello-world", new { controller = "Home", action = "Index" });
            route.RouteHandler = new HelloYetAgainHandler();
        }

        protected void Application_BeginRequest() {
            Host.BeginRequest();
        }

        protected void Application_EndRequest() {
            Host.EndRequest();
        }

        protected void MvcSingletons(ContainerBuilder builder) {
            builder.RegisterInstance(ControllerBuilder.Current);
            builder.RegisterInstance(RouteTable.Routes);
            builder.RegisterInstance(ModelBinders.Binders);
            builder.RegisterInstance(ModelMetadataProviders.Current);
            builder.RegisterInstance(ViewEngines.Engines);
        }

        public static IOrchardHost Host {
            get { return _host; }
        }

        public static void ReloadExtensions() {
            _host.ReloadExtensions();
        }

        public static IStandaloneEnvironment CreateStandaloneEnvironment(string name) {
            var settings = _hostContainer.Resolve<IShellSettingsManager>().LoadSettings().SingleOrDefault(x => x.Name == name);
            return Host.CreateStandaloneEnvironment(settings);
        }
    }
}