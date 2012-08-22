using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {

    public class ListLayoutForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ListLayoutForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "ListLayout",
                        _Options: Shape.Fieldset(
                            Title: T("Order"),
                            _ValueTrue: Shape.Radio(
                                Id: "unordered", Name: "Order",
                                Checked: true,
                                Title: T("Unordered list"), Value: "unordered"
                                ),
                            _ValueFalse: Shape.Radio(
                                Id: "ordered", Name: "Order",
                                Title: T("Ordered list"), Value: "ordered"
                                )
                            ),
                        _HtmlProperties: Shape.Fieldset(
                            Classes: new []{"expando"},
                            Title: T("Html properties"), 
                            _ListId: Shape.TextBox(
                                Id: "list-id", Name: "ListId",
                                Title: T("List id"),
                                Description: T("The id to provide on the list element."),
                                Classes: new[] { "textMedium", "tokenized" }
                                ),
                            _ListClass: Shape.TextBox(
                                Id: "list-class", Name: "ListClass",
                                Title: T("List class"),
                                Description: T("The class to provide on the list element."),
                                Classes: new[] { "textMedium", "tokenized" }
                                ),
                            _ItemClass: Shape.TextBox(
                                Id: "item-class", Name: "ItemClass",
                                Title: T("Item class"),
                                Description: T("The class to provide on each list item."),
                                Classes: new[] { "textMedium", "tokenized" }
                                )
                            )
                        );

                    return f;
                };

            context.Form("ListLayout", form);

        }
    }

    public class ListLayoutFormsValitator : FormHandler {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == "ListLayout") {
                if (context.ValueProvider.GetValue("Order") == null) {
                    context.ModelState.AddModelError("Order", T("You must provide an Order").Text);
                }
            }
        }
    }

}