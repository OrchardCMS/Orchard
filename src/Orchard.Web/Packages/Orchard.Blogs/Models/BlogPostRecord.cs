//using System;
using System;
using Orchard.Models.Records;
using Orchard.Users.Models;

namespace Orchard.Blogs.Models {
    public class BlogPostRecord : ContentPartRecord {
        public virtual BlogRecord Blog { get; set; }
        public virtual UserRecord Creator { get; set; }
        public virtual string Title { get; set; }
        public virtual string Body { get; set; }
        public virtual string BodyShort { get; set; }
        //TODO: (erikpo) Probably need some sort of body source type to keep track of what created the text so its available to plugins to use appropriately.
        public virtual string Slug { get; set; }
        public virtual DateTime? Published { get; set; }
        //public virtual DateTime Created { get; set; }
        //public virtual DateTime Modified { get; set; }
    }
}