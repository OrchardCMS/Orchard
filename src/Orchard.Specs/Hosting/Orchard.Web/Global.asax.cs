using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Orchard.Environment;

namespace Orchard.Specs.Hosting.Orchard.Web
{
    public class MvcApplication : HttpApplication
    {
        private static IOrchardHost _host;

        protected void Application_Start()
        {
            _host = OrchardStarter.CreateHost(MvcSingletons);
            _host.Initialize();

            var route = RouteTable.Routes.MapRoute("foo", "hello-world", new { controller = "Home", action = "Index" });
            route.RouteHandler = new HelloYetAgainHandler();

        }

        protected void Application_BeginRequest()
        {
            _host.BeginRequest();
        }

        protected void Application_EndRequest()
        {
            _host.EndRequest();
        }

        protected void MvcSingletons(ContainerBuilder builder)
        {
            builder.RegisterInstance(ControllerBuilder.Current);
            builder.RegisterInstance(RouteTable.Routes);
            builder.RegisterInstance(ModelBinders.Binders);
            builder.RegisterInstance(ModelMetadataProviders.Current);
            builder.RegisterInstance(ViewEngines.Engines);
        }

    }
}