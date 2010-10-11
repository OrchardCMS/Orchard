using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper) {
            var publish = shapeHelper.Parts_Contents_Publish(ContentPart: part);
            if (!string.IsNullOrWhiteSpace(displayType))
                publish.Metadata.Type = string.Format("{0}.{1}", publish.Metadata.Type, displayType);
            var location = part.GetLocation(displayType, "Secondary", "5");
            return ContentShape(publish).Location(location);
        }
    }
}