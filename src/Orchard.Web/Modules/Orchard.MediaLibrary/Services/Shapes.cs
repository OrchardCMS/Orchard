using System;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.MediaLibrary.Models;

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

                        var mediaPart = contentItem.As<MediaPart>();

                        // Media__[DisplayType] e.g. Media-Summary
                        displaying.ShapeMetadata.Alternates.Add("Media_" + EncodeAlternateElement(displaying.ShapeMetadata.DisplayType));

                        if (!String.IsNullOrEmpty(mediaPart.LogicalType)) {

                            // Media__[LogicalType] e.g. Media-Image,
                            displaying.ShapeMetadata.Alternates.Add("Media__" + EncodeAlternateElement(mediaPart.LogicalType));

                            // Media__[Id] e.g. Media-42,
                            displaying.ShapeMetadata.Alternates.Add("Media__" + contentItem.Id);

                            // Media_[DisplayType]__[LogicalType] e.g. Media-Image.Summary
                            displaying.ShapeMetadata.Alternates.Add("Media_" + displaying.ShapeMetadata.DisplayType + "__" + EncodeAlternateElement(mediaPart.LogicalType));
                        }

                        // Media__[ContentType] e.g. Media-ProductImage,
                        displaying.ShapeMetadata.Alternates.Add("Media__" + EncodeAlternateElement(contentItem.ContentType));

                        // Media__[Id] e.g. Media-42,
                        displaying.ShapeMetadata.Alternates.Add("Media__" + contentItem.Id);

                        // Media_[DisplayType]__[ContentType] e.g. Media-ProductImage.Summary
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
