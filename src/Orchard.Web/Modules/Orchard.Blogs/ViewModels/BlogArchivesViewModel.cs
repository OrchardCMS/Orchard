using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogArchivesViewModel {
        public string Path { get; set; }
        public IEnumerable<BlogPart> Blogs { get; set; }
    }
}