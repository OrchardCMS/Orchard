using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Models {
    public class RecentBlogPostsPart : ContentPart<RecentBlogPostsPartRecord> {

        public string ForBlog {
            get { return Record.BlogSlug; }
            set { Record.BlogSlug = value; }
        }

        [Required]
        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }
    }
}
