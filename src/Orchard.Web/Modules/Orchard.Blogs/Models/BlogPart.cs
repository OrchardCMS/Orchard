using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart<BlogPartRecord> {
        public string Name {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }
    }
}