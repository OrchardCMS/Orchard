using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class AdminBlogsViewModel : AdminViewModel {
        public IEnumerable<AdminBlogEntry> Entries { get; set; }
    }

    public class AdminBlogEntry {
        public ContentItemViewModel<Blog> ContentItemViewModel { get; set; }
        public int TotalPostCount { get; set; }
    }
}