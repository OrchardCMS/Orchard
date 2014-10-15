using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class TextAreaDriver : FormsElementDriver<TextArea> {
        private readonly ITokenizer _tokenizer;
        public TextAreaDriver(IFormManager formManager, ITokenizer tokenizer) : base(formManager) {
            _tokenizer = tokenizer;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "AutoLabel";
                yield return "TextArea";
            }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("TextArea", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TextArea",
                    _Value: shape.Textarea(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The value of this text area.")),
                    _Rows: shape.Textbox(
                        Id: "Rows",
                        Name: "Rows",
                        Title: "Rows",
                        Classes: new[] { "text", "small" },
                        Description: T("The number of rows for this text area.")),
                    _Columns: shape.Textbox(
                        Id: "Columns",
                        Name: "Columns",
                        Title: "Columns",
                        Classes: new[] { "text", "small" },
                        Description: T("The number of columns for this text area.")));

                return form;
            });
        }

        protected override void OnDisplaying(TextArea element, ElementDisplayContext context) {
            context.ElementShape.TokenizedValue = _tokenizer.Replace(element.RuntimeValue, null);
        }
    }
}