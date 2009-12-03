using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogForAdminViewModel : AdminViewModel {
        public Blog Blog { get; set; }
        public IEnumerable<BlogPost> Posts { get; set; }
    }
}