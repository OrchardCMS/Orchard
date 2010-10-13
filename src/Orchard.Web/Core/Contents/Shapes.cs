using System;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.UI.Zones;

namespace Orchard.Core.Contents {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Content")
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.ContentType);
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.Id);
                    }
                });

            builder.Describe("Items_Content_Editor")
               .OnDisplaying(displaying => {
                   ContentItem contentItem = displaying.Shape.ContentItem;
                   if (contentItem != null) {
                       displaying.ShapeMetadata.Alternates.Add("Items_Content_Editor__" + contentItem.ContentType);
                   }
               });
        }
    }
}
