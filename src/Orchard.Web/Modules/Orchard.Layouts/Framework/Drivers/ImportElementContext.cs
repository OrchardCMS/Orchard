using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Framework.Drivers {
    public class ImportElementContext {
        public ImportElementContext() {
            ExportableState = new StateDictionary();
        }

        public ILayoutAspect Layout { get; set; }
        public IElement Element { get; set; }
        public StateDictionary ExportableState { get; set; }
        public IContentImportSession Session { get; set; }
    }
}