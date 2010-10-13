using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Widget")
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        var zoneName = contentItem.As<WidgetPart>().Zone;
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName);
                        //...would like...
                        //displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName + "__" + contentItem.ContentType);
                    }
                });
        }
    }
}
