using System.Web.Routing;

namespace Orchard.Mvc.Routes {
    public class RouteDescriptor {
        public string Name { get; set; }
        public int Priority { get; set; }
        public RouteBase Route { get; set; }
    }
}