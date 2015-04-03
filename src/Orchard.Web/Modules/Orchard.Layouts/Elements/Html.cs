using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Html : ContentElement {
        public override LocalizedString DisplayText {
            get { return T("HTML"); }
        }
    }
}