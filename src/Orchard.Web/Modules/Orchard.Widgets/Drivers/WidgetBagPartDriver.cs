using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Drivers {
    [UsedImplicitly]
    public class WidgetBagPartDriver : ContentPartDriver<WidgetBagPart> {
        protected override DriverResult Editor(WidgetBagPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Widgets_WidegetBagPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Widgets.WidgetBagPart", Model: part));
        }
    }
}