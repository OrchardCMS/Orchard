using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogForAdminViewModel : BaseViewModel {
        public ContentItemViewModel<Blog> Blog { get; set; }
    }
}