using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.ContentManagement {
    public class ContentItemMetadata {
        public string DisplayText { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }
        public RouteValueDictionary CreateRouteValues { get; set; }
        public RouteValueDictionary RemoveRouteValues { get; set; }
        public readonly IList<GroupInfo> DisplayGroupInfo = new List<GroupInfo>();
        public readonly IList<GroupInfo> EditorGroupInfo = new List<GroupInfo>();
    }
}