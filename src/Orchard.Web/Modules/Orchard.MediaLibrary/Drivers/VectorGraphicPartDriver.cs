using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class VectorGraphicPartDriver : ContentPartDriver<VectorGraphicPart> {

        protected override DriverResult Display(VectorGraphicPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_VectorGraphic_Metadata", () => shapeHelper.Parts_VectorGraphic_Metadata()),
                ContentShape("Parts_VectorGraphic_Summary", () => shapeHelper.Parts_VectorGraphic_Summary()),
                ContentShape("Parts_VectorGraphic", () => shapeHelper.Parts_VectorGraphic()),
                ContentShape("Parts_VectorGraphic_SummaryAdmin", () => shapeHelper.Parts_VectorGraphic_SummaryAdmin())
            );
        }
    }
}