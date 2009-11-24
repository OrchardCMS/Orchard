using Orchard.Models;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord> {
        public Blog Blog { get; set; }
    }
}