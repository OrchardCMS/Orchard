using Orchard.Blogs.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogPostViewModel : AdminViewModel {
        public ItemEditorViewModel<BlogPost> BlogPost { get; set; }
    }
}