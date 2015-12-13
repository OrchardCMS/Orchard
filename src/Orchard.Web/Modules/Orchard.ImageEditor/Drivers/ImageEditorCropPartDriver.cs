using Orchard.ContentManagement.Drivers;
using Orchard.ImageEditor.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImageEditorCropPartDriver : ContentPartDriver<ImageEditorPart> {

        protected override DriverResult Display(ImageEditorPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Editor_Crop", () => shapeHelper.Parts_Image_Editor_Crop()),
                ContentShape("Parts_Image_Editor_CropOptions", () => shapeHelper.Parts_Image_Editor_CropOptions())
            );
        }
    }
}