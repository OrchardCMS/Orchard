using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class CreateBlogViewModel : AdminViewModel {
        public ContentItemViewModel<Blog> Blog { get; set; }
    }
}