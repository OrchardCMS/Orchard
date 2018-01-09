using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
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

        protected override void Exporting(DocumentPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Length", part.Length);
        }

        protected override void Importing(DocumentPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Length", length =>
                part.Length = int.Parse(length)
            );
        }
        protected override void Cloning(DocumentPart originalPart, DocumentPart clonePart, CloneContentContext context) {
            clonePart.Length = originalPart.Length;
        }
    }
}