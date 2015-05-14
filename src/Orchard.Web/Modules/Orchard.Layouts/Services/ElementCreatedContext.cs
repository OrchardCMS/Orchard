using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public class ElementCreatedContext : ElementEventContext {
        public Element Element { get; set; }
    }
}