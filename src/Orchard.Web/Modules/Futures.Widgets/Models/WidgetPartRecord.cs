using Orchard.ContentManagement.Records;

namespace Futures.Widgets.Models {
    public class WidgetPartRecord : ContentPartRecord {
        public virtual WidgetsPartRecord Scope { get; set; }
        public virtual string Zone { get; set; }
        public virtual string Position { get; set; }
    }
}