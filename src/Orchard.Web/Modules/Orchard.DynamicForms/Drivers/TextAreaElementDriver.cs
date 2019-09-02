using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class TextAreaElementDriver : FormsElementDriver<TextArea> {
        private readonly ITokenizer _tokenizer;
        public TextAreaElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(TextArea element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel", "Properties:1");
            var placeholderEditor = BuildForm(context, "Placeholder", "Properties:10");
            var textAreaEditor = BuildForm(context, "TextArea", "Properties:15");
            var textAreaValidation = BuildForm(context, "TextAreaValidation", "Validation:10");

            return Editor(context, autoLabelEditor, placeholderEditor, textAreaEditor, textAreaValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("TextArea", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TextArea",
                    _Value: shape.Textarea(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "large" },
                        Description: T("The value of this text area.")),
                    _Rows: shape.Textbox(
                        Id: "Rows",
                        Name: "Rows",
                        Title: "Rows",
                        Classes: new[] { "text", "small" },
                        Description: T("The number of rows for this text area.")),
                    _Columns: shape.Textbox(
                        Id: "Columns",
                        Name: "Columns",
                        Title: "Columns",
                        Classes: new[] { "text", "small" },
                        Description: T("The number of columns for this text area.")));

                return form;
            });

            context.Form("TextAreaValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TextAreaValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsRequired",
                        Name: "IsRequired",
                        Title: "Required",
                        Value: "true",
                        Description: T("Check to make this text area a required field.")),
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

        protected override void OnDisplaying(TextArea element, ElementDisplayingContext context) {
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