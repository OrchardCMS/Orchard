using System;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {

    public class GridLayoutForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public GridLayoutForms(
            IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "GridLayout",
                        _Options: Shape.Fieldset(
                            Title: T("Alignment"),
                            _ValueTrue: Shape.Radio(
                                Id: "horizontal", Name: "Alignment",
                                Title: T("Horizontal"), Value: "horizontal",
                                Checked: true,
                                Description: T("Horizontal alignment will place items starting in the upper left and moving right.")
                                ),
                            _ValueFalse: Shape.Radio(
                                Id: "vertical", Name: "Alignment",
                                Title: T("Vertical"), Value: "vertical",
                                Description: T("Vertical alignment will place items starting in the upper left and moving down.")
                                ),
                            _Colums: Shape.TextBox(
                                Id: "columns", Name: "Columns",
                                Title: T("Number of columns/lines "),
                                Value: 3,
                                Description: T("How many columns (in Horizontal mode) or lines (in Vertical mode) to display in the grid."),
                                Classes: new[] { "small-text", "tokenized" }
                                )
                            ),
                        _HtmlProperties: Shape.Fieldset(
                            Title: T("Html properties"), 
                            _ListId: Shape.TextBox(
                                Id: "grid-id", Name: "GridId",
                                Title: T("Grid id"),
                                Description: T("The id to provide on the table element."),
                                Classes: new[] { "textMedium", "tokenized" }
                                ),
                            _ListClass: Shape.TextBox(
                                Id: "grid-class", Name: "GridClass",
                                Title: T("Grid class"),
                                Description: T("The class to provide on the table element."),
                                Classes: new[] { "textMedium", "tokenized" }
                                ),
                            _ItemClass: Shape.TextBox(
                                Id: "row-class", Name: "RowClass",
                                Title: T("Row class"),
                                Description: T("The class to provide on each row."),
                                Classes: new[] { "textMedium", "tokenized" }
                                )
                            )
                        );

                    return f;
                };

            context.Form("GridLayout", form);

        }
    }

    public class GridLayoutFormsValitator : FormHandler {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == "GridLayout") {
                if (context.ValueProvider.GetValue("Alignment") == null) {
                    context.ModelState.AddModelError("Alignment", T("The field Alignment is required.").Text);
                }

                if (context.ValueProvider.GetValue("Columns") == null) {
                    context.ModelState.AddModelError("Columns", T("The field Columns/Lines is required.").Text);
                }
                else {
                    int value;
                    if (!Int32.TryParse(context.ValueProvider.GetValue("Columns").AttemptedValue, out value)) {
                        context.ModelState.AddModelError("Columns", T("The field Columns/Lines must be a valid number.").Text);
                    }
                }
            }
        }
    }

}