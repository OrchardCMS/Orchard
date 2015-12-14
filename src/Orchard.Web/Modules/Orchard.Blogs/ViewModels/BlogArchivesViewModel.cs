using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogArchivesViewModel {
        public int BlogId { get; set; }
        public IEnumerable<BlogPart> Blogs { get; set; }
    }
}