using System.Collections.Generic;
using System.Web.Routing;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Blogs.Models {
    public class BlogPostProvider : ContentProvider {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { BlogPost.ContentType };
        }

        public BlogPostProvider(IRepository<BlogPostRecord> repository, IContentManager contentManager) {
            Filters.Add(new ActivatingFilter<BlogPost>("blogpost"));
            Filters.Add(new ActivatingFilter<CommonAspect>("blogpost"));
            Filters.Add(new ActivatingFilter<RoutableAspect>("blogpost"));
            Filters.Add(new ActivatingFilter<BodyAspect>("blogpost"));
            Filters.Add(new StorageFilter<BlogPostRecord>(repository));
            OnLoaded<BlogPost>((context, bp) => bp.Blog = contentManager.Get<Blog>(bp.Record.Blog.Id));

            OnGetItemMetadata<BlogPost>((context, bp) => {
                context.Metadata.DisplayText = bp.Title;
                context.Metadata.DisplayRouteValues =
                    new RouteValueDictionary(
                        new {
                            area = "Orchard.Blogs",
                            controller = "BlogPost",
                            action = "Item",
                            blogSlug = bp.Blog.Slug,
                            postSlug = bp.Slug
                        });
                context.Metadata.EditorRouteValues =
                    new RouteValueDictionary(
                        new {
                            area = "Orchard.Blogs",
                            controller = "BlogPost",
                            action = "Edit",
                            blogSlug = bp.Blog.Slug,
                            postSlug = bp.Slug
                        });
            });
        }
    }
}