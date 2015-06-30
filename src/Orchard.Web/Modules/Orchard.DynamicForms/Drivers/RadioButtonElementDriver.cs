using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class RadioButtonElementDriver : FormsElementDriver<RadioButton> {
        private readonly ITokenizer _tokenizer;

        public RadioButtonElementDriver(IFormManager formManager, ITokenizer tokenizer)
            : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "AutoLabel";
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
                        Description: T("The value of this radio button.")));

                return form;
            });
        }

        protected override void OnDisplaying(RadioButton element, ElementDisplayContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData());
            context.ElementShape.ProcessedValue = _tokenizer.Replace(element.Value, context.GetTokenData());
        }
    }
}