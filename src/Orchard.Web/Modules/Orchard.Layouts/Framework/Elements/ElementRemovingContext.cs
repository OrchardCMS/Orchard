using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Layouts.Framework.Elements {
    public class ElementRemovingContext {
        public ElementRemovingContext(Element element, IEnumerable<Element> elements, IEnumerable<Element> removedElements, IContent content) {
            Element = element;
            Elements = elements;
            RemovedElements = removedElements;
            Content = content;
        }

        public IContent Content { get; private set; }
        // All the other elements on the canvas.
        public IEnumerable<Element> Elements { get; set; }
        // All the other removed elements from the canvas (including the current element).
        public IEnumerable<Element> RemovedElements { get; set; }
        public Element Element { get; private set; }
    }
}