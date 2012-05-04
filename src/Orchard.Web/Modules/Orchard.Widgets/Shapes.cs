using System;
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
                        var widgetPart = contentItem.As<WidgetPart>();
                        var zoneName = widgetPart.Zone;

                        widget.Classes.Add("widget-" + contentItem.ContentType.HtmlClassify());
                        widget.Classes.Add("widget-" + zoneName.HtmlClassify());

                        // Widget__[ZoneName] e.g. Widget-SideBar
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + zoneName);

                        // Widget__[ContentType] e.g. Widget-BlogArchive
                        displaying.ShapeMetadata.Alternates.Add("Widget__" + contentItem.ContentType);

                        // using the technical name to add a CSS class and an alternate
                        if (!String.IsNullOrWhiteSpace(widgetPart.Name)) {
                            widget.Classes.Add("widget-" + widgetPart.Name);

                            // Widget__Name__[Name]
                            displaying.ShapeMetadata.Alternates.Add("Widget__Name__" + widgetPart.Name);
                        }

                    }
                });
        }
    }
}
