using Orchard.Blogs.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogEditViewModel : AdminViewModel {
        public ItemEditorViewModel<Blog> Blog { get; set; }
    }
}