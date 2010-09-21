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
            _host.Initialize();

            var route = RouteTable.Routes.MapRoute("foo", "hello-world", new { controller = "Home", action = "Index" });
            route.RouteHandler = new HelloYetAgainHandler();
        }

        protected void Application_BeginRequest() {
            Context.Items["originalHttpContext"] = Context;

            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        protected void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();

            builder.RegisterInstance(ControllerBuilder.Current);
            builder.RegisterInstance(ModelMetadataProviders.Current);
        }

        public static void ReloadExtensions() {
            _host.ReloadExtensions();
        }

        public static IWorkContextScope CreateStandaloneEnvironment(string name) {
            var settings = _hostContainer.Resolve<IShellSettingsManager>().LoadSettings().SingleOrDefault(x => x.Name == name);
            if (settings == null) {
                settings = new ShellSettings {
                    Name = name,
                    State = new TenantState("Uninitialized")
                };
            }

            return _host.CreateStandaloneEnvironment(settings);
        }
    }
}