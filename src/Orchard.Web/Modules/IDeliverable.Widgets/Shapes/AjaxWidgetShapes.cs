using IDeliverable.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Shapes {
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxWidgetShapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Widget").OnDisplaying(context => Ajaxify(context, "Widget__Ajaxified"));
            builder.Describe("Content").OnDisplaying(context => Ajaxify(context, "Content__Ajaxified"));
        }

        private static void Ajaxify(ShapeDisplayingContext context, string ajaxifiedShapeType) {
            var ajaxifyPart = ((IContent)context.Shape.ContentItem).As<AjaxifyPart>();

            if (ajaxifyPart == null || !ajaxifyPart.Ajaxify || context.Shape.Ajaxified == true)
                return;

            context.ShapeMetadata.Type = ajaxifiedShapeType;
        }
    }
}