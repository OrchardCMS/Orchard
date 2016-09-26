using Orchard.ContentManagement.Drivers;
using Orchard.ImageEditor.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImageEditorRotatePartDriver : ContentPartDriver<ImageEditorPart> {

        protected override DriverResult Display(ImageEditorPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Editor_Rotate", () => shapeHelper.Parts_Image_Editor_Rotate()),
                ContentShape("Parts_Image_Editor_RotateOptions", () => shapeHelper.Parts_Image_Editor_RotateOptions())      
            );
        }
    }
}