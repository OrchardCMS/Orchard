using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogArchivesViewModel {
        public Blog Blog { get; set; }
        public IEnumerable<KeyValuePair<ArchiveData, int>> Archives { get; set; }
    }
}