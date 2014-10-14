using Orchard.Layouts.Framework.Elements;

namespace Orchard.DynamicForms.Elements {
    public class ValidationMessage : Element {
        public override string Category {
            get { return "Form"; }
        }

        public override bool HasEditor {
            get { return true; }
        }
    }
}