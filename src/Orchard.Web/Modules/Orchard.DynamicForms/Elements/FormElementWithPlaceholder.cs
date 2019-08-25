using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public abstract class FormElementWithPlaceholder : LabeledFormElement {
        public string Placeholder {
            get { return this.Retrieve(x => x.Placeholder); }
            set { this.Store(x => x.Placeholder, value); }
        }
    }
}