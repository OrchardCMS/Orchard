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
    }
}