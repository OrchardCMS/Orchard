using System;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.UI.Zones;

namespace Orchard.Core.Contents {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Content")
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(name => ContentZone(creating, name))))
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.Id);
                    }
                });
        }

        private static object ContentZone(ShapeCreatingContext creating, string name) {
            var zone = creating.New.ContentZone();
            zone.ZoneName = name;
            return zone;
        }
    }
}
