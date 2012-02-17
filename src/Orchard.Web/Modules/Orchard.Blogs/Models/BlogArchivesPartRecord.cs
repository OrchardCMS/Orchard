using Orchard.ContentManagement.Records;

namespace Orchard.Blogs.Models {
    /// <summary>
    /// The content part used by the BlogArchives widget
    /// </summary>
    public class BlogArchivesPartRecord : ContentPartRecord {
        public virtual int BlogId { get; set; }
    }
}
