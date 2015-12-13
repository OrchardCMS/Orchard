using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {

    public class ShapeLayoutForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ShapeLayoutForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "ShapeLayout",
                        _Options: Shape.Fieldset(
                            Title: T("Shape options"),
                            _ShapeType: Shape.TextBox(
                                Id: "shape-type", Name: "ShapeType",
                                Title: T("Shape type"),
                                Description: T("The type of the shape which is used for rendering content items."),
                                Classes: new[] { "text medium", "tokenized" }
                                )
                            )
                        );

                    return f;
                };

            context.Form("ShapeLayout", form);

        }
    }
}