using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class AudioPartDriver : ContentPartDriver<AudioPart> {
        protected override DriverResult Display(AudioPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Audio_Metadata", () => shapeHelper.Parts_Audio_Metadata()),
                ContentShape("Parts_Audio_SummaryAdmin", () => shapeHelper.Parts_Audio_SummaryAdmin()),
                ContentShape("Parts_Audio_Summary", () => shapeHelper.Parts_Audio_Summary()),
                ContentShape("Parts_Audio", () => shapeHelper.Parts_Audio())
                );
        }

        protected override void Exporting(AudioPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Length", part.Length);
        }

        protected override void Importing(AudioPart part, ContentManagement.Handlers.ImportContentContext context) {
            var length = context.Attribute(part.PartDefinition.Name, "Length");
            if (length != null) {
                part.Length = int.Parse(length);
            }
        }
    }
}