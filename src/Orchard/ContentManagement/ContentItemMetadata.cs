using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.ContentManagement {
    public class ContentItemMetadata {
        private RouteValueDictionary _adminRouteValues;

        public ContentItemMetadata() {
            Identity = new ContentIdentity();
        }
        public string DisplayText { get; set; }
        public ContentIdentity Identity { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }
        public RouteValueDictionary CreateRouteValues { get; set; }
        public RouteValueDictionary RemoveRouteValues { get; set; }
        public RouteValueDictionary AdminRouteValues {
            get { return _adminRouteValues ?? EditorRouteValues; }
            set { _adminRouteValues = value; }
        }
        public readonly IDictionary<string, Func<RouteValueDictionary>> RouteValues = new Dictionary<string, Func<RouteValueDictionary>>();

        public readonly IList<GroupInfo> DisplayGroupInfo = new List<GroupInfo>();
        public readonly IList<GroupInfo> EditorGroupInfo = new List<GroupInfo>();
    }
}