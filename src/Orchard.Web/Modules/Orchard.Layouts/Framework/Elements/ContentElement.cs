using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Framework.Elements {
    public abstract class ContentElement : Element, IContentElement {
        public override string Category {
            get { return "Content"; }
        }

        public virtual string Content {
            get { return this.Retrieve(x => x.Content); }
            set { this.Store(x => x.Content, value); }
        }
    }
}