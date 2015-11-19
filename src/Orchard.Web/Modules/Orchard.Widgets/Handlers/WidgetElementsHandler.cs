using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Helpers;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    [OrchardFeature("Orchard.Widgets.Elements")]
    public class WidgetElementsHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;

        public WidgetElementsHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            OnUpdated<WidgetPart>(PostProcessPlacedWidget);
        }

        private void PostProcessPlacedWidget(UpdateContentContext context, WidgetPart part) {
            if (!part.IsPlaceableContent())
                return;

            // This is a widget placed on a layout, so clear out the zone propertiey
            // to prevent the widget from appearing on the Widgets screen and on the front-end.
            part.Zone = null;

            // To prevent the widget from being recognized as being orphaned, set its container.
            // If the current container is a LayerPart, override that as well.
            var commonPart = part.As<ICommonPart>();
            if (commonPart != null && (commonPart.Container == null || commonPart.Container.Is<LayerPart>())) {
                commonPart.Container = _orchardServices.WorkContext.CurrentSite;
            }
        }
    }
}