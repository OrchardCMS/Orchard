using Orchard.ContentManagement;
using Orchard.ContentPicker.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentPicker.Handlers {
    public class ContentMenuItemPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;

        public ContentMenuItemPartHandler(IContentManager contentManager, IRepository<ContentMenuItemPartRecord> repository) {
            _contentManager = contentManager;
            Filters.Add(new ActivatingFilter<ContentMenuItemPart>("ContentMenuItem"));
            Filters.Add(StorageFilter.For(repository));

            OnLoading<ContentMenuItemPart>((context, part) => part._content.Loader(() => {
                if (part.ContentItemId != null) {
                    return contentManager.Get(part.ContentItemId.Value);
                }

                return null;
            }));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            base.GetItemMetadata(context);

            if (context.ContentItem.ContentType != "ContentMenuItem") {
                return;
            }

            var contentMenuItemPart = context.ContentItem.As<ContentMenuItemPart>();
            // the display route for the menu item is the one for the referenced content item
            if(contentMenuItemPart != null) {

                // if the content doesn't exist anymore
                if(contentMenuItemPart.Content == null) {
                    return;
                }

                context.Metadata.DisplayRouteValues = _contentManager.GetItemMetadata(contentMenuItemPart.Content).DisplayRouteValues;
            }
        }
    }
}