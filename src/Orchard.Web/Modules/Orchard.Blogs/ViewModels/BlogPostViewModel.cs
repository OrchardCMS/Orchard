using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostViewModel : BaseViewModel {
        public Blog Blog { get; set; }
        public ContentItemViewModel<BlogPost> BlogPost { get; set; }
    }
}