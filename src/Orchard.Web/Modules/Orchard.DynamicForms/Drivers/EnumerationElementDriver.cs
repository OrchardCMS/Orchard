using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Helpers;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class EnumerationElementDriver : FormsElementDriver<Enumeration> {
        private readonly ITokenizer _tokenizer;
        public EnumerationElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer)
            : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(Enumeration element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel", "Properties:1");
            var enumerationEditor = BuildForm(context, "Enumeration", "Properties:15");
            var checkBoxValidation = BuildForm(context, "EnumerationValidation", "Validation:10");

            return Editor(context, autoLabelEditor, enumerationEditor, checkBoxValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Enumeration", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Enumeration",
                    _Options: shape.Textarea(
                        Id: "Options",
                        Name: "Options",
                        Title: "Options",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("Enter one option per line. To differentiate between an option's text and value, separate the two by a colon. For example: &quot;Option 1:1&quot;")),
                    _InputType: shape.SelectList(
                        Id: "InputType",
                        Name: "InputType",
                        Title: "Input Type",
                        Description: T("The control to render when presenting the list of options.")),
                    _DefaultValue: shape.Textbox(
                        Id: "DefaultValue",
                        Name: "DefaultValue",
                        Title: "Default Value",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The default value of this enumeration field.")));

                form._InputType.Items.Add(new SelectListItem { Text = T("Select List").Text, Value = "SelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Multi Select List").Text, Value = "MultiSelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Radio List").Text, Value = "RadioList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Check List").Text, Value = "CheckList" });

                return form;
            });

            context.Form("EnumerationValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "EnumerationValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "Required",
                        Name: "Required",
                        Title: "Required",
                        Value: "true",
                        Description: T("Tick this checkbox to make at least one option required.")),
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

        protected override void OnDisplaying(Enumeration element, ElementDisplayingContext context) {
            var typeName = element.GetType().Name;
            var displayType = context.DisplayType;
            var tokenData = context.GetTokenData();

            // Allow the initially selected value to be tokenized.
            // If a value was posted, use that value instead (without tokenizing it).
            if (element.PostedValue == null) {
                var defaultValue = _tokenizer.Replace(element.DefaultValue, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
                element.RuntimeValue = defaultValue;
            }

            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.ProcessedOptions = _tokenizer.Replace(element.Options, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }).ToArray();
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, element.InputType));
        }
    }
}