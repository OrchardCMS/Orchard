using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
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
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Length", length =>
                part.Length = int.Parse(length)
            );
        }
        protected override void Cloning(AudioPart originalPart, AudioPart clonePart, CloneContentContext context) {
            clonePart.Length = originalPart.Length;
        }
    }
}