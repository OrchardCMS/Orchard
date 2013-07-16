using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.MediaLibrary.Services {
    public class Shapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Media")
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        // Alternates in order of specificity. 
                        // Display type > content type > specific content > display type for a content type > display type for specific content
                        // BasicShapeTemplateHarvester.Adjust will then adjust the template name

                        // Media__[DisplayType] e.g. Media-Summary
                        displaying.ShapeMetadata.Alternates.Add("Media_" + EncodeAlternateElement(displaying.ShapeMetadata.DisplayType));

                        // Media__[ContentType] e.g. Media-BlogPost,
                        displaying.ShapeMetadata.Alternates.Add("Media__" + EncodeAlternateElement(contentItem.ContentType));

                        // Media__[Id] e.g. Media-42,
                        displaying.ShapeMetadata.Alternates.Add("Media__" + contentItem.Id);

                        // Media_[DisplayType]__[ContentType] e.g. Media-Image.Summary
                        displaying.ShapeMetadata.Alternates.Add("Media_" + displaying.ShapeMetadata.DisplayType + "__" + EncodeAlternateElement(contentItem.ContentType));

                        // Media_[DisplayType]__[Id] e.g. Media-42.Summary
                        displaying.ShapeMetadata.Alternates.Add("Media_" + displaying.ShapeMetadata.DisplayType + "__" + contentItem.Id);
                    }
                });
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames 
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private string EncodeAlternateElement(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }

    }
}
