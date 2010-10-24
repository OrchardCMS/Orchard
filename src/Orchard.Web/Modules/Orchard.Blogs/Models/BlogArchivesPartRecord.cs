using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Orchard.Blogs.Models {
    /// <summary>
    /// The content part used by the BlogArchives widget
    /// </summary>
    public class BlogArchivesPartRecord : ContentPartRecord {
        public const ushort DefaultBlogSlugLength = 255;

        [StringLength(DefaultBlogSlugLength)]
        [Required]
        public virtual string BlogSlug { get; set; }
    }
}
