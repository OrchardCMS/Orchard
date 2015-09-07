using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Models;
using Orchard.Widgets.Models;

namespace Orchard.Layouts.Handlers {
    public class WidgetPartHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;

        public WidgetPartHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            OnUpdating<WidgetPart>(PreProcessPlacedWidget);
            OnUpdated<WidgetPart>(PostProcessPlacedWidget);
        }

        private void PreProcessPlacedWidget(UpdateContentContext context, WidgetPart part) {
            if (!part.IsPlaceableContent())
                return;

            // This widget will be placed on a layout and thus will be created outside
            // the context of the widget admin controller, which would have provided a default position value.
            // Since the position property is required, we need to set it here to prevent a model validation error when updating.
            part.Position = "0";
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