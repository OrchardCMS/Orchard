using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Models {
    public class ContentItemMetadata {
        public string DisplayText { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }

        public IEnumerable<string> DisplayTabs { get; set; }
        public IEnumerable<string> EditorTabs { get; set; }
    }
}