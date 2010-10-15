namespace Orchard.Blogs.ViewModels {
    public class DisplayBlogViewModel {
        public dynamic Blog { get; set; }
        public dynamic BlogPostList { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}