using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Futures.Widgets.Models {
    public class Widget : ContentPart<WidgetRecord> {
    }

    public class WidgetRecord : ContentPartRecord {
        public virtual HasWidgetsRecord Scope { get; set; }
        public virtual string Zone { get; set; }
        public virtual string Position { get; set; }
    }
}