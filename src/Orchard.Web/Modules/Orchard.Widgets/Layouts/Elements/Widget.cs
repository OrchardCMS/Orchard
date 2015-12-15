using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Widgets.Layouts.Elements {
    [OrchardFeature("Orchard.Widgets.Elements")]
    public class Widget : Element {
        public override string Category {
            get { return "Widgets"; }
        }

        public override bool IsSystemElement {
            get { return true; }
        }

        public override bool HasEditor {
            get { return false; }
        }

        public int? WidgetId {
            get { return this.Retrieve(x => x.WidgetId); }
            set { this.Store(x => x.WidgetId, value); }
        }
    }
}