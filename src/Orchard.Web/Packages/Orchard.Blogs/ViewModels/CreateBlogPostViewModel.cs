using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogPostViewModel : AdminViewModel {
        public ItemViewModel<BlogPost> BlogPost { get; set; }
    }
}