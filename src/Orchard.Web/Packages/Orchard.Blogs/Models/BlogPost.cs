using System;
using Orchard.Core.Common.Models;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Blogs.Models {
    public class BlogPost : ContentPart<BlogPostRecord> {
        //public Blog Blog { get; set; }
        public string Title { get { return this.As<RoutableAspect>().Title; } }
        public string Body { get { return this.As<BodyAspect>().Body; } }
        public string Slug { get { return this.As<RoutableAspect>().Slug; } }
        public IUser Creator { get { return this.As<CommonAspect>().OwnerField.Value; } }
        public DateTime? Published { get { return Record.Published; } }
    }
}