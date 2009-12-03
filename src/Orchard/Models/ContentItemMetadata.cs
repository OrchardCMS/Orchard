using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Models {
    public class ContentItemMetadata {
        public string DisplayText { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }

        public IEnumerable<string> DisplayGroups { get; set; }
        public IEnumerable<string> EditorGroups { get; set; }
    }
}