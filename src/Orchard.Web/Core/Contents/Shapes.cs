using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Core.Contents {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Content")
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        //Content-BlogPost
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.ContentType);
                        //Content-42
                        displaying.ShapeMetadata.Alternates.Add("Items_Content__" + contentItem.Id);
                        //Content.Summary
                        displaying.ShapeMetadata.Alternates.Add("Items_Content_" + displaying.ShapeMetadata.DisplayType);
                        //Content.Summary-Page
                        displaying.ShapeMetadata.Alternates.Add("Items_Content_" + displaying.ShapeMetadata.DisplayType + "__" + contentItem.ContentType);
                    }
                });

            builder.Describe("Items_Content_Editor")
               .OnDisplaying(displaying => {
                   ContentItem contentItem = displaying.Shape.ContentItem;
                   if (contentItem != null) {
                       //Content.Editor-Page
                       displaying.ShapeMetadata.Alternates.Add("Items_Content_Editor__" + contentItem.ContentType);
                   }
               });
        }
    }
}
