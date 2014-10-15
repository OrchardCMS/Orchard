using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Services {
    public class ElementCreatedContext : ElementEventContext {
        public IElement Element { get; set; }
    }
}