using Orchard.ContentManagement.Drivers;
using Orchard.ImageEditor.Models;

namespace Orchard.ImageEditor.Drivers {
    public class ImageEditorEffectsPartDriver : ContentPartDriver<ImageEditorPart> {

        protected override DriverResult Display(ImageEditorPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Image_Editor_Effects", () => shapeHelper.Parts_Image_Editor_Effects()),
                ContentShape("Parts_Image_Editor_EffectsOptions", () => shapeHelper.Parts_Image_Editor_EffectsOptions())      
            );
        }
    }
}