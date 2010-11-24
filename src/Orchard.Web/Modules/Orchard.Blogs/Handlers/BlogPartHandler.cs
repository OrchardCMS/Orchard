using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartHandler : ContentHandler {
        public BlogPartHandler(IRepository<BlogPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<BlogPart>((context, blog) => {
                                            context.Shape.Description = blog.Description;
                                            context.Shape.PostCount = blog.PostCount;
                                        });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blog = context.ContentItem.As<BlogPart>();

            if (blog == null)
                return;

            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Create"}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Edit"},
                {"Id", context.ContentItem.Id}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Remove"},
                {"Id", context.ContentItem.Id}
            };
        }
    }
}