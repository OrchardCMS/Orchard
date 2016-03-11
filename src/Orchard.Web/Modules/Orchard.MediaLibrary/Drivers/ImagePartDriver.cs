using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class ImagePartDriver : ContentPartDriver<ImagePart> {

        protected override DriverResult Display(ImagePart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Metadata", () => shapeHelper.Parts_Image_Metadata()),
                ContentShape("Parts_Image_Summary", () => shapeHelper.Parts_Image_Summary()),
                ContentShape("Parts_Image", () => shapeHelper.Parts_Image()),
                ContentShape("Parts_Image_SummaryAdmin", () => shapeHelper.Parts_Image_SummaryAdmin())
            );
        }

        protected override void Exporting(ImagePart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Height", part.Height);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Width", part.Width);
        }

        protected override void Importing(ImagePart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Height", height =>
                part.Height = int.Parse(height)
            );

            context.ImportAttribute(part.PartDefinition.Name, "Width", width =>
                part.Width = int.Parse(width)
            );
        }
    }
}