using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Models.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogViewModel : BaseViewModel {
        public Blog Blog { get; set; }
        public IEnumerable<ItemDisplayViewModel<BlogPost>> Posts { get; set; }
    }
}