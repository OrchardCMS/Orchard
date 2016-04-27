using System;
using System.Collections.Generic;
using Orchard.Conditions.Services;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services.Models;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class RadioButtonElementDriver : FormsElementDriver<RadioButton> {
        private readonly ITokenizer _tokenizer;

        public RadioButtonElementDriver(IFormsBasedElementServices formsServices, IConditionManager conditionManager, ITokenizer tokenizer)
            : base(formsServices, conditionManager, tokenizer) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "AutoLabel";
                yield return "Editable";
                yield return "RadioButton";
            }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("RadioButton", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "RadioButton",
                    Description: T("The label for this radio button."),
                        _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The value of this radio button.")),
                    _DefaultValue:
                        shape.Checkbox(
                        Id: "DefaultValue",
                        Name: "DefaultValue",
                        Title: "Default Value",
                        Value: "true",
                        Description: T("Sets default value to unchecked or checked.")));
                return form;
            });
        }

        protected override void OnDisplaying(RadioButton element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData(), new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.ProcessedValue = _tokenizer.Replace(element.Value, context.GetTokenData());
            context.ElementShape.Disabled = ((context.DisplayType != "Design") && !String.IsNullOrWhiteSpace(element.ReadOnlyRule) && EvaluateRule(element.ReadOnlyRule, new { Element = element }));
        }
    }
}