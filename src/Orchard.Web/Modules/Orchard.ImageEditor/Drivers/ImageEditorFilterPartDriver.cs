using Orchard.ContentManagement.Drivers;
using Orchard.ImageEditor.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImageEditorFilterPartDriver : ContentPartDriver<ImageEditorPart> {

        protected override DriverResult Display(ImageEditorPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Editor_Filter", () => shapeHelper.Parts_Image_Editor_Filter()),
                ContentShape("Parts_Image_Editor_FilterOptions", () => shapeHelper.Parts_Image_Editor_FilterOptions())      
            );
        }
    }
}