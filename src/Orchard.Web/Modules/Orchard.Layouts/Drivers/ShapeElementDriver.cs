using System;
using System.Collections.Generic;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Drivers {
    public class ShapeElementDriver : FormsElementDriver<Shape> {
        private readonly IShapeFactory _shapeFactory;

        public ShapeElementDriver(IFormsBasedElementServices formsServices, IShapeFactory shapeFactory)
            : base(formsServices) {
            _shapeFactory = shapeFactory;
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "ShapeElement"; }
        }

        protected override void OnDisplaying(Shape element, ElementDisplayingContext context) {
            if (String.IsNullOrWhiteSpace(element.ShapeType))
                return;

            var shape = _shapeFactory.Create(element.ShapeType);
            context.ElementShape.Shape = shape;
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("ShapeElement", shapeFactory => {
                var factory = (dynamic)shapeFactory;
                var form = factory.Fieldset(
                    Id: "ShapeElement",
                    _ShapeType: factory.Textbox(
                        Id: "shapeType",
                        Name: "ShapeType",
                        Title: T("Shape Type"),
                        Description: T("The shape type name to dislay."),
                        Classes: new[] { "text", "large", "tokenized" }));

                return form;
            });
        }
    }
}