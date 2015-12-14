using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    [OrchardFeature("Orchard.Blogs.RemotePublishing")]
    public class RemoteBlogPublishingDriver : ContentPartDriver<BlogPart> {
        protected override DriverResult Display(BlogPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_RemotePublishing", shape => shape.Blog(part));
        }
    }
}