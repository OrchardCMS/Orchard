using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Blogs.Services;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Blogs.Models {
    public class BlogPostHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { BlogPost.ContentType };
        }

        public BlogPostHandler(
            IRepository<BlogPostRecord> repository,
            IContentManager contentManager,
            IBlogPostService blogPostService) {

            Filters.Add(new ActivatingFilter<BlogPost>("blogpost"));
            Filters.Add(new ActivatingFilter<CommonAspect>("blogpost"));
            Filters.Add(new ActivatingFilter<RoutableAspect>("blogpost"));
            Filters.Add(new ActivatingFilter<BodyAspect>("blogpost"));
            Filters.Add(new StorageFilter<BlogPostRecord>(repository));


            OnCreated<BlogPost>((context, bp) => bp.Blog.PostCount++);
        }

    }
}