using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Widgets.Models;

namespace Orchard.Widgets {
    public class Shapes : IShapeTableProvider {
        private readonly IOrchardServices _orchardServices;

        public Shapes(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Widget")
                .Configure(descriptor => {
                    // todo: have "alternates" for chrome
                    if (_orchardServices.Authorizer.Authorize(Permissions.ManageWidgets))
                        descriptor.Wrappers.Add("Widget_Manage");
                    else
                        descriptor.Wrappers.Add("Widget");
                })
                .OnCreated(created => {
                    var widget = created.Shape;
                    widget.Main.Add(created.New.PlaceChildContent(Source: widget));
                })
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        var zoneName = contentItem.As<WidgetPart>().Zone;
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName);
                        //...would like...if '__' was collapsible
                        //displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName + "__" + contentItem.ContentType);
                    }
                });
        }
    }
}
