using System.Web.Routing;

namespace Orchard.Mvc {
    public class RouteDescriptor {
        public string Name { get; set; }
        public int Priority { get; set; }
        public RouteBase Route { get; set; }
    }
}