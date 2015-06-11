using Orchard.ContentManagement;

namespace Orchard.Blogs.Models {
    /// <summary>
    /// The content part used by the BlogArchives widget
    /// </summary>
    public class BlogArchivesPart : ContentPart<BlogArchivesPartRecord> {

        public int BlogId {
            get { return Record.BlogId; }
            set { Record.BlogId = value; }
        }
    }
}
