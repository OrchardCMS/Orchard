using Orchard.AntiSpam.Models;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Elements;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    [OrchardFeature("Orchard.DynamicForms.AntiSpam")]
    public class ReCaptchaElementDriver : FormsElementDriver<ReCaptcha>{
        private readonly IOrchardServices _services;
        private readonly ITokenizer _tokenizer;

        public ReCaptchaElementDriver(IFormsBasedElementServices formsServices, IOrchardServices services, ITokenizer tokenizer) : base(formsServices) {
            _services = services;
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(ReCaptcha element, ElementEditorContext context) {
            var reCaptchaValidation = BuildForm(context, "ReCaptchaValidation", "Validation:10");
            return Editor(context, reCaptchaValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("ReCaptchaValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "ReCaptchaValidation",
                    _CustomValidationMessage: shape.Textbox(
                        Id: "CustomValidationMessage",
                        Name: "CustomValidationMessage",
                        Title: "Custom Validation Message",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("Optionally provide a custom validation message.")),
                    _ShowValidationMessage: shape.Checkbox(
                        Id: "ShowValidationMessage",
                        Name: "ShowValidationMessage",
                        Title: "Show Validation Message",
                        Value: "true",
                        Description: T("Autogenerate a validation message when a validation error occurs for the current anti-spam filter. Alternatively, to control the placement of the validation message you can use the ValidationMessage element instead.")));

                return form;
            });
        }

        protected override void OnDisplaying(ReCaptcha element, ElementDisplayingContext context) {
            var workContext = _services.WorkContext;
            var currentSite = workContext.CurrentSite;
            var settings = currentSite.As<ReCaptchaSettingsPart>();

            if (settings.TrustAuthenticatedUsers && workContext.CurrentUser != null) {
                return;
            }

            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.PublicKey = settings.PublicKey;
        }
    }
}