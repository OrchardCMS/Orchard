using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class OEmbedPartDriver : ContentPartDriver<OEmbedPart> {
        protected override DriverResult Display(OEmbedPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_OEmbed_Metadata", () => shapeHelper.Parts_OEmbed_Metadata()),
                ContentShape("Parts_OEmbed_Summary", () => shapeHelper.Parts_OEmbed_Summary()),
                ContentShape("Parts_OEmbed_SummaryAdmin", () => shapeHelper.Parts_OEmbed_SummaryAdmin()),
                ContentShape("Parts_OEmbed", () => shapeHelper.Parts_OEmbed())
            );
        }

        protected override void Exporting(OEmbedPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Source", part.Source);
        }

        protected override void Importing(OEmbedPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Source", source =>
                part.Source = source
            );
        }
    }
}