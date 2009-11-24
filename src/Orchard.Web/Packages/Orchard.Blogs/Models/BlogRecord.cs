using System.Collections.Generic;
using Orchard.Models.Records;

namespace Orchard.Blogs.Models {
    public class BlogRecord : ContentPartRecord {
        public virtual IEnumerable<BlogPostRecord> Posts { get; set; }
        public virtual string Description { get; set; }
        //public virtual bool Enabled { get; set; }
    }
}