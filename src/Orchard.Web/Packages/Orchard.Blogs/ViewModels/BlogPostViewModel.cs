using Orchard.Blogs.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostViewModel : BaseViewModel {
        public Blog Blog { get; set; }
        public ItemDisplayViewModel<BlogPost> BlogPost { get; set; }
    }
}