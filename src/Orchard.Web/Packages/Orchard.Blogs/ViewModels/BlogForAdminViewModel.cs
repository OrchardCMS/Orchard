using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogForAdminViewModel : AdminViewModel {
        public ItemViewModel<Blog> Blog { get; set; }
    }
}