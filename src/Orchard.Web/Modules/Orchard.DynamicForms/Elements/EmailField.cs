using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements
{
    public class EmailField : FormElementWithPlaceholder
    {
        public EmailFieldValidationSettings ValidationSettings => Data.GetModel<EmailFieldValidationSettings>("");
    }
}