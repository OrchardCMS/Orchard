using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.ContentManagement {
    public class ContentItemMetadata {
        public ContentItemMetadata(IContent item) {
            DisplayRouteValues = GetDisplayRouteValues(item);
            EditorRouteValues = GetEditorRouteValues(item);
            CreateRouteValues = GetCreateRouteValues(item);
        }

        public string DisplayText { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }
        public RouteValueDictionary CreateRouteValues { get; set; }
        public RouteValueDictionary RemoveRouteValues { get; set; }

        public IEnumerable<string> DisplayGroups { get; set; }
        public IEnumerable<string> EditorGroups { get; set; }

        private static RouteValueDictionary GetDisplayRouteValues(IContent item) {
            return new RouteValueDictionary {
                {"Area", "Contents"},
                {"Controller", "Item"},
                {"Action", "Display"},
                {"id", item.ContentItem.Id}
            };
        }

        private static RouteValueDictionary GetEditorRouteValues(IContent item) {
            return new RouteValueDictionary {
                {"Area", "Contents"},
                {"Controller", "Admin"},
                {"Action", "Edit"},
                {"id", item.ContentItem.Id}
            };
        }

        private static RouteValueDictionary GetCreateRouteValues(IContent item) {
            return new RouteValueDictionary {
                {"Area", "Contents"},
                {"Controller", "Admin"},
                {"Action", "Create"},
                {"id", item.ContentItem.ContentType}
            };
        }

        private static RouteValueDictionary GetRemoveRouteValues(IContent item) {
            return new RouteValueDictionary {
                {"Area", "Contents"},
                {"Controller", "Admin"},
                {"Action", "Remove"}
            };
        }
    }
}