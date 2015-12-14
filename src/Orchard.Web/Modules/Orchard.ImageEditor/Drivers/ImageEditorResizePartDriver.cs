using Orchard.ContentManagement.Drivers;
using Orchard.ImageEditor.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImageEditorResizePartDriver : ContentPartDriver<ImageEditorPart> {

        protected override DriverResult Display(ImageEditorPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Editor_Resize", () => shapeHelper.Parts_Image_Editor_Resize()),
                ContentShape("Parts_Image_Editor_ResizeOptions", () => shapeHelper.Parts_Image_Editor_ResizeOptions())      
            );
        }
    }
}