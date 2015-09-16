using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class ValidationMessageElementDriver : FormsElementDriver<ValidationMessage> {
        private readonly ITokenizer _tokenizer;

        public ValidationMessageElementDriver(IFormManager formManager, ITokenizer tokenizer) : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "ValidationMessage"; }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("ValidationMessage", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "ValidationMessage",
                    _ValidationMessageFor: shape.Textbox(
                        Id: "For",
                        Name: "For",
                        Title: "For",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The name of the field this validation message is for.")));

                return form;
            });
        }

        protected override void OnDisplaying(ValidationMessage element, ElementDisplayContext context) {
            context.ElementShape.ProcessedFor = _tokenizer.Replace(element.For, context.GetTokenData());
        }
    }
}