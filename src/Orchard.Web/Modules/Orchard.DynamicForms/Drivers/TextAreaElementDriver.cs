using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class TextAreaElementDriver : FormsElementDriver<TextArea> {
        private readonly ITokenizer _tokenizer;
        public TextAreaElementDriver(IFormManager formManager, ITokenizer tokenizer) : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(TextArea element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var textAreaEditor = BuildForm(context, "TextArea");
            var textAreaValidation = BuildForm(context, "TextAreaValidation", "Validation:10");

            return Editor(context, autoLabelEditor, textAreaEditor, textAreaValidation);
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
                        Classes: new[] { "text", "large", "tokenized" },
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

        protected override void OnDisplaying(TextArea element, ElementDisplayContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData());
            context.ElementShape.ProcessedValue = _tokenizer.Replace(element.RuntimeValue, context.GetTokenData());
        }
    }
}