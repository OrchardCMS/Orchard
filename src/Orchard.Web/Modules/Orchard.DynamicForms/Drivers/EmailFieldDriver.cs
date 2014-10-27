using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Drivers;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class EmailFieldDriver : FormsElementDriver<EmailField>{
        public EmailFieldDriver(IFormManager formManager) : base(formManager) {}

        protected override EditorResult OnBuildEditor(EmailField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var emailFieldValidation = BuildForm(context, "EmailFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, emailFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
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
    }
}