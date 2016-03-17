using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class HiddenFieldElementDriver : FormsElementDriver<HiddenField> {
        private readonly ITokenizer _tokenizer;
        public HiddenFieldElementDriver(IFormsBasedElementServices formsServices, ITokenizer tokenizer) : base(formsServices) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "HiddenField"; }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("HiddenField", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "HiddenField",
                    _Span: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The value of this hidden field.")));

                return form;
            });
        }

        protected override void OnDisplaying(HiddenField element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedValue = element.RuntimeValue;
        }
    }
}