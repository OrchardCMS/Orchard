using Orchard.Layouts.Framework.Elements;

namespace Orchard.DynamicForms.Elements {
    public class ValidationSummary : Element {
        public override string Category {
            get { return "Forms"; }
        }

        public override bool HasEditor {
            get { return false; }
        }
    }
}