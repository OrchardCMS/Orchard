using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogViewModel : BaseViewModel {
        public ContentItemViewModel<BlogPart> Blog { get; set; }
    }
}