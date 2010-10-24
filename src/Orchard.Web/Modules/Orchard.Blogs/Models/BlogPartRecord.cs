using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Blogs.Models {
    public class BlogPartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string Description { get; set; }
        public virtual int PostCount { get; set; }
    }
}