using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    public class BlogArchivesPartHandler : ContentHandler {
        public BlogArchivesPartHandler(IRepository<BlogArchivesPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}