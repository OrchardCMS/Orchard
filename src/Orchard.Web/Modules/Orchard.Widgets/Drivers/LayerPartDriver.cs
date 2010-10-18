using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Drivers {

    [UsedImplicitly]
    public class LayerPartDriver : ContentPartDriver<LayerPart> {

        protected override DriverResult Editor(LayerPart layerPart, dynamic shapeHelper) {
            return ContentShape("Parts_Widgets_LayerPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Widgets.LayerPart", Model: layerPart));
        }

        protected override DriverResult Editor(LayerPart layerPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(layerPart, Prefix, null, null);
            return Editor(layerPart, shapeHelper);
        }
    }
}