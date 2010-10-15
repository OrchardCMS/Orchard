using Orchard.ContentManagement;

namespace Orchard.Blogs.Models {
    public class BlogPagerPart : ContentPart {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string BlogSlug { get; set; }
        public bool ThereIsANextPage { get; set; }
    }
}