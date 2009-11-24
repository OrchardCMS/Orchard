using Orchard.Core.Common.Models;
using Orchard.Models;

namespace Orchard.Blogs.Models {
    public class Blog : ContentPart<BlogRecord> {
        public int Id { get { return ContentItem.Id; } }
        public string Name { get { return this.As<RoutableAspect>().Title; } }
        public string Slug { get { return this.As<RoutableAspect>().Slug; } }
        public string Description { get { return Record.Description; } }
        //public bool Enabled { get { return Record.Enabled; } }
    }
}