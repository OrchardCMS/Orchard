using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class EmailFieldElementDriver : FormsElementDriver<EmailField>{
        private readonly ITokenizer _tokenizer;

        public EmailFieldElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(EmailField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel", "Properties:1");
            var placeholderEditor = BuildForm(context, "Placeholder", "Properties:10");
            var emailFieldEditor = BuildForm(context, "EmailField", "Properties:15");
            var emailFieldValidation = BuildForm(context, "EmailFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, placeholderEditor, emailFieldEditor, emailFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("EmailField", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "EmailField",
                    _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "medium" },
                        Description: T("The value of this email field.")));

                return form;
            });

            context.Form("EmailFieldValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "EmailFieldValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsRequired",
                        Name: "IsRequired",
                        Title: "Required",
                        Value: "true",
                        Description: T("Tick this checkbox to make this email field a required field.")),
                    _MaximumLength: shape.Textbox(
                        Id: "MaximumLength",
                        Name: "MaximumLength",
                        Title: "Maximum Length",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The maximum length allowed.")),
                    _CompareWith: shape.Textbox(
                        Id: "CompareWith",
                        Name: "CompareWith",
                        Title: "Compare With",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The name of another field whose value must match with this email field.")),
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

        protected override void OnDisplaying(EmailField element, ElementDisplayingContext context) {
            var tokenData = context.GetTokenData();
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.ProcessedPlaceholder = _tokenizer.Replace(element.Placeholder, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

            // Allow the initial value to be tokenized.
            // If a value was posted, use that value instead (without tokenizing it).
            context.ElementShape.ProcessedValue = element.PostedValue != null ? element.PostedValue : _tokenizer.Replace(element.RuntimeValue, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
        }
    }
}