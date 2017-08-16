using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class PasswordField : PlaceholderableFormElement {
        public PasswordFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<PasswordFieldValidationSettings>(""); }
        }
    }
}