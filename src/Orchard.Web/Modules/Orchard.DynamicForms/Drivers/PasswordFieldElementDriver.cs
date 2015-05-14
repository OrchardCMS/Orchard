using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Drivers;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class PasswordFieldElementDriver : FormsElementDriver<PasswordField>{
        public PasswordFieldElementDriver(IFormManager formManager) : base(formManager) {}

        protected override EditorResult OnBuildEditor(PasswordField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var passwordFieldValidation = BuildForm(context, "PasswordFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, passwordFieldValidation);
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
    }
}