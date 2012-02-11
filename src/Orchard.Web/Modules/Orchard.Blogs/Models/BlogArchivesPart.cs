using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Blogs.Models {
    /// <summary>
    /// The content part used by the BlogArchives widget
    /// </summary>
    public class BlogArchivesPart : ContentPart<BlogArchivesPartRecord> {
        [Required]
        public string ForBlog {
            get { return Record.BlogSlug; }
            set { Record.BlogSlug = value; }
        }
    }
}
