using Orchard.ContentManagement;

namespace Orchard.Layouts.Framework.Elements {
    public class ElementRemovingContext {
        public ElementRemovingContext(Element element, IContent content) {
            Element = element;
            Content = content;
        }

        public IContent Content { get; private set; }
        public Element Element { get; private set; }
    }
}