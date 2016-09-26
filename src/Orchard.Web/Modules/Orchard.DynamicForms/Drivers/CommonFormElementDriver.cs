using System.Collections.Generic;
using Orchard.DisplayManagement;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.DynamicForms.Drivers {
    public class CommonFormElementDriver : FormsElementDriver<FormElement> {

        public CommonFormElementDriver(IFormManager formManager, IShapeFactory shapeFactory) : base(formManager) {
            New = shapeFactory;
        }

        public override int Priority {
            get { return 500; }
        }

        protected override IEnumerable<string> FormNames {
            get { yield return "CommonFormElement"; }
        }

        public dynamic New { get; set; }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("CommonFormElement", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "CommonFormElement",
                    _Span: shape.Textbox(
                        Id: "InputName",
                        Name: "InputName",
                        Title: "Name",
                        Classes: new[] { "text", "medium", "tokenized" },
                        Description: T("The name of this form field.")),
                    _FormBindingContentType: shape.Hidden(
                        Id: "FormBindingContentType",
                        Name: "FormBindingContentType"));

                return form;
            });
        }

        protected override void OnDisplaying(FormElement element, ElementDisplayContext context) {
            context.ElementShape.Metadata.Wrappers.Add("FormElement_Wrapper");
            context.ElementShape.Child.Add(New.PlaceChildContent(Source: context.ElementShape));

        }

    }
}