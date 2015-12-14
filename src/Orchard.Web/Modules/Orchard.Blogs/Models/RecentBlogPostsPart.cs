using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Models {
    public class RecentBlogPostsPart : ContentPart<RecentBlogPostsPartRecord> {

        public int BlogId {
            get { return Record.BlogId; }
            set { Record.BlogId = value; }
        }

        [Required]
        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }
    }
}
