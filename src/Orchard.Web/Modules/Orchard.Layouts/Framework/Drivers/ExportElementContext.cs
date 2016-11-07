using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.Framework.Drivers {
    public class ExportElementContext {
        public ExportElementContext(Element element, ILayoutAspect layout, ElementDataDictionary exportableData) {
            Element = element;
            Layout = layout;
            ExportableData = exportableData;
        }

        public ILayoutAspect Layout { get; private set; }
        public Element Element { get; private set; }
        public ElementDataDictionary ExportableData { get; private set; }
    }
}