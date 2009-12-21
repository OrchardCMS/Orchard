using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public interface IItemDriver : IEvents {
        void GetItemMetadata(GetItemMetadataContext context);
    }

    public abstract class ItemDriver<TItemPart> : PartDriver<TItemPart>, IItemDriver where TItemPart : class, IContent {
        public void GetItemMetadata(GetItemMetadataContext context) {
            var item = context.ContentItem.As<TItemPart>();
            if (item != null) {
                context.Metadata.DisplayText = GetDisplayText(item) ?? context.Metadata.DisplayText;
                context.Metadata.DisplayRouteValues = GetDisplayRouteValues(item) ?? context.Metadata.DisplayRouteValues;
                context.Metadata.EditorRouteValues = GetEditorRouteValues(item) ?? context.Metadata.EditorRouteValues;
            }
        }

        protected virtual string GetDisplayText(TItemPart item) { return null; }
        protected virtual RouteValueDictionary GetDisplayRouteValues(TItemPart item) { return null; }
        protected virtual RouteValueDictionary GetEditorRouteValues(TItemPart item) { return null; }
    }
}
