using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Layouts.Services {
    /// <summary>
    /// Represents a text document of a layout.
    /// </summary>
    public class LayoutDocument {
        public LayoutDocument() {
            Elements = new List<ElementDocument>();
        }
        public IList<ElementDocument> Elements { get; set; }

        public string ToString(string separator) {
            return String.Join(separator, Elements.Select(x => x.Content));
        }
    }
}