using System.Collections.Generic;
using Orchard.Forms.Services;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.Layouts.Drivers {
    public class ColumnElementDriver : FormsElementDriver<Column> {
        public ColumnElementDriver(IFormManager formManager) : base(formManager) {
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "Column";
            }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Column", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Column",
                    _Span: shape.Textbox(
                        Id: "ColumnSpan",
                        Name: "ColumnSpan",
                        Title: "Span",
                        Description: T("The column span.")),
                    _Offset: shape.Textbox(
                        Id: "ColumnOffset",
                        Name: "ColumnOffset",
                        Title: "Offset",
                        Description: T("The column offset expressed in span size.")));

                return form;
            });
        }
    }
}