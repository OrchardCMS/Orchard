using Orchard.Blogs.Models;

namespace Orchard.Blogs.ViewModels {
    public class DisplayBlogViewModel {
        public BlogPart BlogPart { get; set; }
        public dynamic BlogPostList { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}