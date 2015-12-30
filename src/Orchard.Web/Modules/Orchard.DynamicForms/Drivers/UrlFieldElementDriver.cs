﻿using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class UrlFieldElementDriver : FormsElementDriver<UrlField> {
        private readonly ITokenizer _tokenizer;

        public UrlFieldElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(UrlField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var webAddressFieldEditor = BuildForm(context, "UrlField");
            var webAddressFieldValidation = BuildForm(context, "UrlFieldValidation", "Validation:10");

            return Editor(context, autoLabelEditor, webAddressFieldEditor, webAddressFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("UrlField", factory => {
                var shape = (dynamic) factory;
                var form = shape.Fieldset(
                    Id: "UrlField",
                    _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] {"text", "medium", "tokenized"},
                        Description: T("The value of this URL field.")));

                return form;
            });

            context.Form("UrlFieldValidation", factory => {
                var shape = (dynamic) factory;
                var form = shape.Fieldset(
                    Id: "UrlFieldValidation",
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
                        Classes: new[] {"text", "medium", "tokenized"},
                        Description: T("The maximum length allowed.")),
                    _CustomValidationMessage: shape.Textbox(
                        Id: "CustomValidationMessage",
                        Name: "CustomValidationMessage",
                        Title: "Custom Validation Message",
                        Classes: new[] {"text", "large", "tokenized"},
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

        protected override void OnDisplaying(UrlField element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData(), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.ProcessedValue = element.RuntimeValue;
        }
    }
}