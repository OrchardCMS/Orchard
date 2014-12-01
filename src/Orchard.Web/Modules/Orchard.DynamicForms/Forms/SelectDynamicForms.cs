using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.DynamicForms.Forms {
    public class SelectDynamicForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SelectDynamicForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.Form("SelectDynamicForms", factory => {
                var shape = (dynamic)factory;
                 var form = shape.Form(
                    Id: "AnyOfDynamicForms",
                    _Parts: Shape.Textbox(
                        Id: "dynamicforms",
                        Name: "DynamicForms",
                        Title: T("Dynamic Forms"),
                        Description: T("Enter a comma separated list of dynamic form names. Leave empty to handle all forms."),
                        Classes: new[] { "text", "large" })
                    );

                return form;
            });

        }
    }
}