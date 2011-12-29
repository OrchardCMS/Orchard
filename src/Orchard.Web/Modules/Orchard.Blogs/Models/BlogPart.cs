using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Blogs.Models {
    public class BlogPart : ContentPart<BlogPartRecord> {

        // TODO: (PH) This isn't referenced in many places but should use ContentItemMetadata instead?
        public string Name {
            get { return this.As<ITitleAspect>().Title; }
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