using System.Collections.Generic;
using Orchard.Indexing;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Framework.Elements {
    public class LayoutIndexingContext {
        public ILayoutAspect Layout { get; set; }
        public IEnumerable<IElement> Elements { get; set; }
        public IDocumentIndex DocumentIndex { get; set; }
    }
}