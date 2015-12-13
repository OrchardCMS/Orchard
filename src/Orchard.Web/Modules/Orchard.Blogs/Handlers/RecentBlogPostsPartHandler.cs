using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class RecentBlogPostsPartHandler : ContentHandler {
        public RecentBlogPostsPartHandler(IRepository<RecentBlogPostsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}