using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class ButtonElementDriver : FormsElementDriver<Button> {
        private readonly ITokenizer _tokenizer;

        public ButtonElementDriver(IFormManager formManager, ITokenizer tokenizer) : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "Button"; }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Button", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Form",
                    _ButtonText: shape.Textbox(
                        Id: "Text",
                        Name: "Text",
                        Title: "Text",
                        Value: "Submit",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The button text.")));

                return form;
            });
        }

        protected override void OnDisplaying(Button element, ElementDisplayContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedText = _tokenizer.Replace(element.Text, context.GetTokenData());
        }
    }
}