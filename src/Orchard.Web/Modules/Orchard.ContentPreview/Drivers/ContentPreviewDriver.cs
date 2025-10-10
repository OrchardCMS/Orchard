using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.ContentPreview.Drivers {
    public class ContentPreviewDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper) =>
            ContentShape("ContentPreview_Button", () => shapeHelper.ContentPreview_Button(Model: part.ContentItem));
    }
}