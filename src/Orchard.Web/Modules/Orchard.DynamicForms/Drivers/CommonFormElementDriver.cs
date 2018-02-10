using System.Collections.Generic;
using Orchard.DisplayManagement;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Drivers {
    public class CommonFormElementDriver : FormsElementDriver<FormElement> {

        public CommonFormElementDriver(IFormsBasedElementServices formsServices, IShapeFactory shapeFactory) : base(formsServices) {
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

        protected override void OnDisplaying(FormElement element, ElementDisplayingContext context) {
            context.ElementShape.Metadata.Wrappers.Add("FormElement_Wrapper");
            context.ElementShape.Child.Add(New.PlaceChildContent(Source: context.ElementShape));
        }

        protected override EditorResult OnBuildEditor(FormElement element, ElementEditorContext context) {
            return Editor(context, BuildForm(context, "CommonFormElement", "Properties:10"));
        }
    }
}