using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Contents.Handlers {
    public class ContentsModuleHandler : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.Metadata.EditorRouteValues == null) {
                context.Metadata.EditorRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Edit"},
                    {"Id", context.ContentItem.Id}
                };
            }
            if (context.Metadata.DisplayRouteValues == null) {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"Id", context.ContentItem.Id}
                };
            }
        }
    }
}
