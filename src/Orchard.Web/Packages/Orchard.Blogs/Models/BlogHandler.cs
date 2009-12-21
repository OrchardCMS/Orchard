using System.Collections.Generic;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Models {
    public class BlogHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { Blog.ContentType };
        }

        public BlogHandler(IRepository<BlogRecord> repository) {
            Filters.Add(new ActivatingFilter<Blog>("blog"));
            Filters.Add(new ActivatingFilter<CommonAspect>("blog"));
            Filters.Add(new ActivatingFilter<RoutableAspect>("blog"));
            Filters.Add(new StorageFilter<BlogRecord>(repository));
            Filters.Add(new ContentItemTemplates<Blog>("Items/Blogs.Blog", "Summary DetailAdmin SummaryAdmin"));
        }
    }
}
