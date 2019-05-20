using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Orchard.ContentPreview.Drivers {
    public class ContentPreviewDriver : ContentPartDriver<CommonPart> {
        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) =>
            ContentShape("ContentPreview_Button", () => shapeHelper.ContentPreview_Button());
    }
}