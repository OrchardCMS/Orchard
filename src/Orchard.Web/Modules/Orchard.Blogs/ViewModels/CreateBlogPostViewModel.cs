using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogPostViewModel : BaseViewModel {
        public ContentItemViewModel<BlogPost> BlogPost { get; set; }
    }
}