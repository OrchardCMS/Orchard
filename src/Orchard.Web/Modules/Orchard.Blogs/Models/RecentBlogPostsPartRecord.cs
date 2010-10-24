using Orchard.ContentManagement.Records;

namespace Orchard.Blogs.Models {
    public class RecentBlogPostsPartRecord : ContentPartRecord {
        public virtual string BlogSlug { get; set; }
        public virtual int Count { get; set; }
    }
}
