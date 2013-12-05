using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart<BlogPartRecord> {

        public string Name {
            get { return this.As<ITitleAspect>().Title; }
        }

        public string Description {
            get { return Retrieve(x => x.Description); }
            set { Store(x => x.Description, value); }
        }

        public int PostCount {
            get { return Retrieve(x => x.PostCount); }
            set { Store(x => x.PostCount, value); }
        }
    }
}