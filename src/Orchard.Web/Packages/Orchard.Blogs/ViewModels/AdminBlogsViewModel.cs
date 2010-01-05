using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class AdminBlogsViewModel : AdminViewModel {
        public IEnumerable<ItemViewModel<Blog>> Blogs { get; set; }
    }
}