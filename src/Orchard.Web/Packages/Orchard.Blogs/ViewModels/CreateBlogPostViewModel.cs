using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogPostViewModel : AdminViewModel {
        public ContentItemViewModel<BlogPost> BlogPost { get; set; }
    }
}