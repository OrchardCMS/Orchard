using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class CheckboxElementDriver : FormsElementDriver<CheckBox> {
        private readonly ITokenizer _tokenizer;

        public CheckboxElementDriver(IFormManager formManager, ITokenizer tokenizer)
            : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(CheckBox element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var checkBoxEditor = BuildForm(context, "CheckBox");
            var checkBoxValidation = BuildForm(context, "CheckBoxValidation", "Validation:10");

            return Editor(context, autoLabelEditor, checkBoxEditor, checkBoxValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("CheckBox", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "CheckBox",
                    _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The value of this checkbox.")));

                return form;
            });

            context.Form("CheckBoxValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "CheckBoxValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsMandatory",
                        Name: "IsMandatory",
                        Title: "Mandatory",
                        Value: "true",
                        Description: T("Tick this checkbox to make this check box element mandatory.")),
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

        protected override void OnDisplaying(CheckBox element, ElementDisplayContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData());
            context.ElementShape.ProcessedValue = _tokenizer.Replace(element.RuntimeValue, context.GetTokenData());
        }
    }
}