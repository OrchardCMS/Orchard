using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Blogs.Models {
    public class BlogProvider : ContentProvider {
        public BlogProvider(IRepository<BlogRecord> repository) {
            Filters.Add(new ActivatingFilter<Blog>("blog"));
            Filters.Add(new ActivatingFilter<CommonAspect>("blog"));
            Filters.Add(new ActivatingFilter<RoutableAspect>("blog"));
            Filters.Add(new StorageFilter<BlogRecord>(repository));
        }
    }
}