using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImagePartDriver : ContentPartDriver<ImagePart> {

        protected override DriverResult Display(ImagePart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Image_Editor", () => shapeHelper.Parts_Image_Editor());
        }
    }
}