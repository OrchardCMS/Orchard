using Orchard.ContentManagement.Records;

namespace Orchard.Blogs.Models {
    public class RecentBlogPostsPartRecord : ContentPartRecord {
        public RecentBlogPostsPartRecord() {
            Count = 5;
        }

        public virtual int BlogId { get; set; }
        public virtual int Count { get; set; }
    }
}
