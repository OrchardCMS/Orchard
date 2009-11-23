using System.Collections.Generic;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogsViewModel {
        public IEnumerable<Blog> Blogs { get; set; }
    }
}