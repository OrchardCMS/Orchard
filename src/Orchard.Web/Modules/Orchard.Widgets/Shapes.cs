using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Widget")
                .Configure(descriptor => {
                    // todo: have "alternates" for chrome
                    //todo: (heskew) something...this still doesn't feel right
                    descriptor.Wrappers.Add("Widget_ControlWrapper");
                    descriptor.Wrappers.Add("Widget_Wrapper");
                })
                .OnCreated(created => {
                    var widget = created.Shape;
                    widget.Main.Add(created.New.PlaceChildContent(Source: widget));
                })
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        var zoneName = contentItem.As<WidgetPart>().Zone;
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + zoneName);
                    }
                });
        }
    }
}
