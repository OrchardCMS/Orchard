using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartHandler : ContentHandler {
        private readonly IBlogPathConstraint _blogPathConstraint;

        public BlogPartHandler(IRepository<BlogPartRecord> repository, IBlogPathConstraint blogPathConstraint) {
            _blogPathConstraint = blogPathConstraint;
            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<BlogPart>((context, blog) => {
                context.Shape.Description = blog.Description;
                context.Shape.PostCount = blog.PostCount;
            });

            OnPublished<BlogPart>((context, blog) => _blogPathConstraint.AddPath(blog.As<IAliasAspect>().Path));
            OnUnpublished<BlogPart>((context, blog) => _blogPathConstraint.RemovePath(blog.As<IAliasAspect>().Path));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blog = context.ContentItem.As<BlogPart>();

            if (blog == null)
                return;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "Blog"},
                {"Action", "Item"},
                {"blogId", context.ContentItem.Id}
            };
            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Create"}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Edit"},
                {"blogId", context.ContentItem.Id}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Remove"},
                {"blogId", context.ContentItem.Id}
            };
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Item"},
                {"blogId", context.ContentItem.Id}
            };
        }
    }
}