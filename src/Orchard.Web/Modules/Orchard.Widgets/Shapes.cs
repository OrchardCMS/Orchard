using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Widget")
                .Configure(descriptor => {
                    // todo: have "alternates" for chrome
                    descriptor.Wrappers.Add("Widget_Wrapper");
                    descriptor.Wrappers.Add("Widget_ControlWrapper");
                })
                .OnCreated(created => {
                    var widget = created.Shape;
                    widget.Main.Add(created.New.PlaceChildContent(Source: widget));
                })
                .OnDisplaying(displaying => {
                    var widget = displaying.Shape;
                    widget.Classes.Add("widget");
                    ContentItem contentItem = widget.ContentItem;
                    if (contentItem != null) {
                        var zoneName = contentItem.As<WidgetPart>().Zone;
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName);
                    }
                });
        }
    }
}
