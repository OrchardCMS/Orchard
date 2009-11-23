using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogsViewModel : AdminViewModel {
        public IEnumerable<Blog> Blogs { get; set; }
    }
}