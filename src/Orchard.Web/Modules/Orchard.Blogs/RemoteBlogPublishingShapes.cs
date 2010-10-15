using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;

namespace Orchard.Blogs {
    [OrchardFeature("Remote Blog Publishing")]
    public class RemoteBlogPublishingShapes : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Items_Content__Blog")
                .OnDisplaying(displaying => {
                    if (displaying.ShapeMetadata.DisplayType == "Detail") {
                        displaying.ShapeMetadata.Wrappers.Add("RemoteBlogPublishing");
                    }
                });
        }
    }
}
