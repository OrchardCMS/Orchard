using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class TextField : LabeledFormElement {
        public TextFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<TextFieldValidationSettings>(""); }
        }
    }
}