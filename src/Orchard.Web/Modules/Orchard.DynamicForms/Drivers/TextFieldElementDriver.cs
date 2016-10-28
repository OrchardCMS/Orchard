using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class TextFieldElementDriver : FormsElementDriver<TextField>{
        private readonly ITokenizer _tokenizer;

        public TextFieldElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(TextField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var textFieldEditor = BuildForm(context, "TextField");
            var textFieldValidation = BuildForm(context, "TextFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, textFieldEditor, textFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("TextField", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TextField",
                    _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "medium" },
                        Description: T("The value of this text field.")));

                return form;
            });

            context.Form("TextFieldValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TextFieldValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsRequired",
                        Name: "IsRequired",
                        Title: "Required",
                        Value: "true",
                        Description: T("Check to make this text field a required field.")),
                    _MinimumLength: shape.Textbox(
                        Id: "MinimumLength",
                        Name: "MinimumLength",
                        Title: "Minimum Length",
                        Classes: new[] { "text", "medium" },
                        Description: T("The minimum length required.")),
                    _MaximumLength: shape.Textbox(
                        Id: "MaximumLength",
                        Name: "MaximumLength",
                        Title: "Maximum Length",
                        Classes: new[] { "text", "medium" },
                        Description: T("The maximum length allowed.")),
                    _ValidationExpression: shape.Textbox(
                        Id: "ValidationExpression",
                        Name: "ValidationExpression",
                        Title: "Validation Expression",
                        Classes: new[] { "text", "large" },
                        Description: T("The regular expression the text must match with.")),
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

        protected override void OnDisplaying(TextField element, ElementDisplayingContext context) {
            var tokenData = context.GetTokenData();
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

            // Allow the initial value to be tokenized.
            // If a value was posted, use that value instead (without tokenizing it).
            context.ElementShape.ProcessedValue = element.PostedValue != null ? element.PostedValue : _tokenizer.Replace(element.RuntimeValue, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
        }
    }
}