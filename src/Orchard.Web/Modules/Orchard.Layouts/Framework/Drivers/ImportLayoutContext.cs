using Orchard.Layouts.Models;

namespace Orchard.Layouts.Framework.Drivers {
    public class ImportLayoutContext {
        public ILayoutAspect Layout { get; set; }
        public IContentImportSession Session { get; set; }
    }
}