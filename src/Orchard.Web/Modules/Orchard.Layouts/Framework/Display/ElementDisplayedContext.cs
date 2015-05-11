using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Display {
    public class ElementDisplayedContext {
        public IContent Content { get; set; }
        public Element Element { get; set; }
        public string DisplayType { get; set; }
        public dynamic ElementShape { get; set; }
        public IUpdateModel Updater { get; set; }
    }
}