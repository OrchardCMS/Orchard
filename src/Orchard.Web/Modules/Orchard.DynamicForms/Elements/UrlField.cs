using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements {
    public class UrlField : FormElementWithPlaceholder {
        public UrlFieldValidationSettings ValidationSettings {
            get { return Data.GetModel<UrlFieldValidationSettings>(""); }
        }
    }
}