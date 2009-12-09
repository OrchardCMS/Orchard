using Orchard.Blogs.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostEditViewModel : AdminViewModel {
        public Blog Blog { get; set; }
        public ItemEditorViewModel<BlogPost> BlogPost { get; set; }
    }
}