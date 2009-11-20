using Orchard.Models;

namespace Orchard.Blogs.Models {
    public class Blog : ContentPartForRecord<BlogRecord> {
        public string Name { get { return Record.Name; } }
        public string Slug { get { return Record.Slug; } }
        public bool Enabled { get { return Record.Enabled; } }
    }
}