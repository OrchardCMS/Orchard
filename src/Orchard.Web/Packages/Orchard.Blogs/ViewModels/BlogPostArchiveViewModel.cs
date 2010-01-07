using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Blogs.ViewModels {
    public class BlogPostArchiveViewModel : BaseViewModel {
        public Blog Blog { get; set; }
        public ArchiveData ArchiveData { get; set; }
        public IEnumerable<ContentItemViewModel<BlogPost>> BlogPosts { get; set; }
    }
}