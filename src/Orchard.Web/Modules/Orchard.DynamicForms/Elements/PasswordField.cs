using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class PasswordField : LabeledFormElement {
        public PasswordFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<PasswordFieldValidationSettings>(""); }
        }
    }
}