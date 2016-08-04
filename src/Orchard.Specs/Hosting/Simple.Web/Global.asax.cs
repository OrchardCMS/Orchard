using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Specs.Hosting.Orchard.Web;

namespace Orchard.Specs.Hosting.Simple.Web {
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {

            var route = RouteTable.Routes.MapRoute("foo", "hello-world", new { controller = "Home", action = "Index" });
            route.RouteHandler = new HelloYetAgainHandler();

        }

    }
}