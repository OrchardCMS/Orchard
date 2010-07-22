using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogHandler : ContentHandler {
        public BlogHandler(IRepository<BlogPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}