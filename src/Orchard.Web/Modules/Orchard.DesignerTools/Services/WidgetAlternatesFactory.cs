using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using Orchard.Widgets.Models;

namespace Orchard.DesignerTools.Services {
    [OrchardFeature("WidgetAlternates")]
    public class WidgetAlternatesFactory : ShapeDisplayEvents {
        public override void Displaying(ShapeDisplayingContext context) {
            context.ShapeMetadata.OnDisplaying(displayedContext => {
                // We don't want the "Widget" content item itself, but the content item that consists of the Widget part (e.g. Parts.Blogs.RecentBlogPosts)
                if (displayedContext.ShapeMetadata.Type != "Widget") {
                    // look for ContentItem property
                    ContentItem contentItem = displayedContext.Shape.ContentItem;

                    // if not, check for ContentPart 
                    if (contentItem == null) {
                        ContentPart contentPart = displayedContext.Shape.ContentPart;
                        if (contentPart != null) {
                            contentItem = contentPart.ContentItem;
                        }
                    } 
                    
                    if (contentItem != null) {
                        // Is the contentItem a widget? (we could probably test for the stereotype setting, don't know if that is more efficient than selecting the WidgetPart)
                        var widgetPart = contentItem.As<WidgetPart>();
                        if (widgetPart != null) {
                            var zoneName = widgetPart.Zone;
                            var shapeName = displayedContext.ShapeMetadata.Type;
                            var contentTypeName = contentItem.ContentType;

                            // Add 2 alternates for flexible widget shape naming:
                            // [ShapeName]-[ZoneName].cshtml: (e.g. "Parts.Blogs.RecentBlogPosts-myZoneName.cshtml")
                            // [ShapeName]-[ContentTypeName]-[ZoneName].cshtml: (e.g. "Parts.Common.Body-RecentBlogPosts-myZoneName.cshtml")
                            displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentTypeName + "__" + zoneName);
                            displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + zoneName);
                        }
                    }
                }
            });

        }
    }
}