using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class DocumentPartDriver : ContentPartDriver<DocumentPart> {
        protected override DriverResult Display(DocumentPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Document_Metadata", () => shapeHelper.Parts_Document_Metadata()),
                ContentShape("Parts_Document_Summary", () => shapeHelper.Parts_Document_Summary()),
                ContentShape("Parts_Document_SummaryAdmin", () => shapeHelper.Parts_Document_SummaryAdmin()),
                ContentShape("Parts_Document", () => shapeHelper.Parts_Document())
            );
        }
    }
}