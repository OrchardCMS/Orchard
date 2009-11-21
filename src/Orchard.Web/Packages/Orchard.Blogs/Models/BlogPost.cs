using Orchard.Models;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPartForRecord<BlogPostRecord> {
        public string BlogSlug { get { return Record.Blog.Slug; } }
        public string Title { get { return Record.Title; } }
        public string Slug { get { return Record.Slug; } }
    }
}