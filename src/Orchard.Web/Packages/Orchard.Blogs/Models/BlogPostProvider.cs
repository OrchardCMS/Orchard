using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Blogs.Models {
    public class BlogPostProvider : ContentProvider {
        public BlogPostProvider(IRepository<BlogPostRecord> repository) {
            Filters.Add(new ActivatingFilter<BlogPost>("blogpost"));
            Filters.Add(new StorageFilter<BlogPostRecord>(repository));
        }
    }
}