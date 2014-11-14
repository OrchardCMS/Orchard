using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    /// <summary>
    /// Represents a layout document node.
    /// </summary>
    public class ElementDocument {
        public IElement Element { get; set; }
        public string Content { get; set; }
    }
}