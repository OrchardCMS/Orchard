using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements
{
    public class UrlField : FormElementWithPlaceholder
    {
        public UrlFieldValidationSettings ValidationSettings => Data.GetModel<UrlFieldValidationSettings>("");
    }
}