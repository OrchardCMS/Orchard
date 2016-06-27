using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Canvas : Container {

        public override string Category {
            get { return "Layout"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Canvas"); }
        }

        public override bool IsSystemElement {
            get { return true; }
        }

        public override bool HasEditor {
            get { return false; }
        }
    }
}