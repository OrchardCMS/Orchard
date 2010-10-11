using JetBrains.Annotations;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Drivers {
    [UsedImplicitly]
    public class WidgetBagPartDriver : ContentPartDriver<WidgetBagPart> {
        private const string TemplateName = "Parts/Widgets.WidgetBagPart";

        protected override DriverResult Editor(WidgetBagPart part, dynamic shapeHelper) {
            var location = part.GetLocation("Editor");
            return ContentPartTemplate("", TemplateName, Prefix).Location(location);
        }
    }
}