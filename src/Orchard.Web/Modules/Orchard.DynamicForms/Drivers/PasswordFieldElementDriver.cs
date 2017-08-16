using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class PasswordFieldElementDriver : FormsElementDriver<PasswordField>{
        private readonly ITokenizer _tokenizer;

        public PasswordFieldElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(PasswordField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel", "Properties:1");
            var placeholderEditor = BuildForm(context, "Placeholder", "Properties:10");
            var passwordFieldValidation = BuildForm(context, "PasswordFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, placeholderEditor, passwordFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("PasswordFieldValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "PasswordFieldValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsRequired",
                        Name: "IsRequired",
                        Title: "Required",
                        Value: "true",
                        Description: T("Tick this checkbox to make this password field a required field.")),
                    _MinimumLength: shape.Textbox(
                        Id: "MinimumLength",
                        Name: "MinimumLength",
                        Title: "Minimum Length",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The minimum length required.")),
                    _MaximumLength: shape.Textbox(
                        Id: "MaximumLength",
                        Name: "MaximumLength",
                        Title: "Maximum Length",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The maximum length allowed.")),
                    _RegularExpression: shape.Textbox(
                        Id: "RegularExpression",
                        Name: "RegularExpression",
                        Title: "Regular Expression",
                        Classes: new[] { "text", "large"},
                        Description: T("The regular expression the password must match with.")),
                    _CompareWith: shape.Textbox(
                        Id: "CompareWith",
                        Name: "CompareWith",
                        Title: "Compare With",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The name of another field whose value must match with this password field.")),
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
                        Description: T("Autogenerate a validation message when a validation error occurs for the current field. Alternatively, to control the placement of the validation message you can use the ValidationMessage element instead.")));

                return form;
            });
        }

        protected override void OnDisplaying(PasswordField element, ElementDisplayingContext context) {
            var tokenData = context.GetTokenData();
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.ProcessedPlaceholder = _tokenizer.Replace(element.Placeholder, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
        }
    }
}