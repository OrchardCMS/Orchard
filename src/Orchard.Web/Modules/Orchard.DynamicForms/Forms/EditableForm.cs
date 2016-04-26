using Orchard.Forms.Services;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Forms {
    public class EditableForm : Component, IFormProvider {
    
        public void Describe(DescribeContext context) {
            context.Form("Editable", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "Editable",
                    _ReadOnlyRule: shape.Textbox(
                        Id: "ReadOnlyRule",
                        Name: "ReadOnlyRule",
                        Title: "Read-Only Rule",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("Evaluates if the field is read-only for the user")//,
                        //EnabledBy: "CreateContent"
                        ));

                return form;
            });
        }
    }
}