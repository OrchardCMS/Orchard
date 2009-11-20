using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Blogs.Models {
    public class BlogPostHandler : ContentHandler {
        public BlogPostHandler(IRepository<BlogPostRecord> repository) {
            Filters.Add(new ActivatingFilter<BlogPost>("blogpost"));
            Filters.Add(new StorageFilterForRecord<BlogPostRecord>(repository));
        }
    }
}