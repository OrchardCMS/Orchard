using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.DynamicForms.Drivers {
    public class LabelElementDriver : FormsElementDriver<Label> {
        public LabelElementDriver(IFormManager formManager) : base(formManager) {}

        protected override IEnumerable<string> FormNames {
            get { yield return "Label"; }
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("Label", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Label",
                    _LabelText: shape.Textbox(
                        Id: "LabelText",
                        Name: "LabelText",
                        Title: "Text",
                        Classes: new[] { "text", "large" },
                        Description: T("The label text.")),
                    _LabelFor: shape.Textbox(
                        Id: "LabelFor",
                        Name: "LabelFor",
                        Title: "For",
                        Classes: new[] { "text", "large" },
                        Description: T("The name of the field this label is for.")));

                return form;
            });
        }
    }
}