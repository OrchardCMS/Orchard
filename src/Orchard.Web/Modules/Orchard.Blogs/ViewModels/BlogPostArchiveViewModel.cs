using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostArchiveViewModel {
        public BlogPart BlogPart { get; set; }
        public IEnumerable<KeyValuePair<ArchiveData, int>> Archives { get; set; }
    }
}