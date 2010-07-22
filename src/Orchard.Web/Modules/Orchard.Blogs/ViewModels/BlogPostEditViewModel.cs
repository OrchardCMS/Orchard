using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostEditViewModel : BaseViewModel {
        public ContentItemViewModel<BlogPostPart> BlogPost { get; set; }
    }
}