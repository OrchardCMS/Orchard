using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.Environment {
    public class HostContext {
        public ControllerBuilder ControllerBuilder { get; set; }
        public RouteCollection Routes { get; set; }
    }
}