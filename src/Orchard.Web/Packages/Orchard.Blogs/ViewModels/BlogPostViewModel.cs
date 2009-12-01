using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Models;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostViewModel : BaseViewModel {
        public Blog Blog { get; set; }
        public BlogPost Post { get; set; }
        public IEnumerable<ModelTemplate> Displays { get; set; }
    }
}