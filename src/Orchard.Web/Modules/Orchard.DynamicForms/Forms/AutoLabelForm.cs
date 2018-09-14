using Orchard.Forms.Services;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Forms {
    public class AutoLabelForm : Component, IFormProvider {
    
        public void Describe(DescribeContext context) {
            context.Form("AutoLabel", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "AutoLabel",
                     _ShowLabel: shape.Checkbox(
                        Id: "ShowLabel",
                        Name: "ShowLabel",
                        Title: "Show Label",
                        Value: "true",
                        Description: T("Check this to show a label for this text field.")),
                    _Label: shape.Textbox(
                        Id: "Label",
                        Name: "Label",
                        Title: "Label",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The label text to render.")),
                    _AutoLabelScript: shape.AutoLabelScript());

                return form;
            });
        }
    }
}