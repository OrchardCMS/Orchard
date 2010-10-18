using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {

    [UsedImplicitly]
    public class WidgetPartDriver : ContentPartDriver<WidgetPart> {
        private readonly IWidgetsService _widgetsService;

        public WidgetPartDriver(IWidgetsService widgetsService) {
            _widgetsService = widgetsService;
        }

        protected override DriverResult Editor(WidgetPart widgetPart, dynamic shapeHelper) {
            widgetPart.AvailableZones = _widgetsService.GetZones();

            return ContentShape("Parts_Widgets_WidgetPart",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Widgets.WidgetPart", Model: widgetPart));
        }

        protected override DriverResult Editor(WidgetPart widgetPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(widgetPart, Prefix, null, null);
            return Editor(widgetPart, shapeHelper);
        }
    }
}