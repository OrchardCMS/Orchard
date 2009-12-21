using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Blogs.Models {
    public class BlogPostRecord : ContentPartRecord {
        public virtual DateTime? Published { get; set; }
    }
}