using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Drivers {

    [UsedImplicitly]
    public class LayerPartDriver : ContentPartDriver<LayerPart> {

        protected override DriverResult Editor(LayerPart layerPart, dynamic shapeHelper) {
            ContentLocation location = layerPart.GetLocation("Editor");
            return ContentPartTemplate(layerPart, "Parts/Widgets.LayerPart").Location(location);
        }

        protected override DriverResult Editor(LayerPart layerPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(layerPart, Prefix, null, null);
            return Editor(layerPart, shapeHelper);
        }
    }
}