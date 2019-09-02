using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class EmailField : FormElementWithPlaceholder {
        public EmailFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<EmailFieldValidationSettings>(""); }
        }
    }
}