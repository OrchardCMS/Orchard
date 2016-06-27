using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class VideoPartDriver : ContentPartDriver<VideoPart> {
        protected override DriverResult Display(VideoPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Video_Metadata", () => shapeHelper.Parts_Video_Metadata()),
                ContentShape("Parts_Video_SummaryAdmin", () => shapeHelper.Parts_Video_SummaryAdmin()),
                ContentShape("Parts_Video_Summary", () => shapeHelper.Parts_Video_Summary()),
                ContentShape("Parts_Video", () => shapeHelper.Parts_Video())
                );
        }

        protected override void Exporting(VideoPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Length", part.Length);
        }

        protected override void Importing(VideoPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Length", length =>
                part.Length = int.Parse(length)
            );
        }
    }
}