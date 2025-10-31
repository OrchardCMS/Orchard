using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Elements
{
    public class ReCaptcha : FormElement
    {
        public ReCaptchaValidationSettings ValidationSettings => Data.GetModel<ReCaptchaValidationSettings>("");
    }
}