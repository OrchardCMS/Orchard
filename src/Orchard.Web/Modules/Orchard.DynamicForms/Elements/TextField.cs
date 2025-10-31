using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements
{
    public class TextField : FormElementWithPlaceholder
    {
        public TextFieldValidationSettings ValidationSettings => Data.GetModel<TextFieldValidationSettings>("");
    }
}