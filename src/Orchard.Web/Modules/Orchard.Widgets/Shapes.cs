using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Widget")
                .Configure(descriptor => {
                    // todo: have "alternates" for chrome
                    descriptor.Wrappers.Add("Widget_Wrapper");
                    descriptor.Wrappers.Add("Widget_ControlWrapper");
                })
                .OnCreated(created => {
                    var widget = created.Shape;
                    widget.Child.Add(created.New.PlaceChildContent(Source: widget));
                })
                .OnDisplaying(displaying => {
                    var widget = displaying.Shape;
                    widget.Classes.Add("widget");

                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        widget.Classes.Add("widget-" + contentItem.ContentType.HtmlClassify());

                        var zoneName = contentItem.As<WidgetPart>().Zone;

                        // Widget__[ZoneName] e.g. Widget-SideBar
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + zoneName);

                        // Widget__[ContentType] e.g. Widget-BlogArchive
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + contentItem.ContentType);
                    }
                });
        }
    }
}
