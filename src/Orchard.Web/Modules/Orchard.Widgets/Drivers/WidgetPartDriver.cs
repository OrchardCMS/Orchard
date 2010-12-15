using System.Collections.Generic;
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
            widgetPart.AvailableLayers = _widgetsService.GetLayers();

            var results = new List<DriverResult> {
                ContentShape("Parts_Widgets_WidgetPart",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts.Widgets.WidgetPart", Model: widgetPart, Prefix: Prefix))
            };

            if (widgetPart.Id > 0)
                results.Add(ContentShape("Widget_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(WidgetPart widgetPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(widgetPart, Prefix, null, null);
            return Editor(widgetPart, shapeHelper);
        }
    }
}