using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Drivers {
    [UsedImplicitly]
    public class WidgetBagPartDriver : ContentPartDriver<WidgetBagPart> {
        private const string TemplateName = "Parts/Widgets.WidgetBagPart";

        protected override DriverResult Editor(WidgetBagPart part, dynamic shapeHelper) {
            //var location = part.GetLocation("Editor");
            // TODO: andrerod convert to new shape API. Location code kept for reference.
            return ContentPartTemplate("", TemplateName, Prefix); //.Location(location);
        }
    }
}