using System;
using System.Globalization;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.PropertyEditors.Forms {
    public class NumericPropertyForm : IFormProvider {

        public const string FormName = "NumericProperty";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public NumericPropertyForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        _Options: Shape.Fieldset(
                            _ValueTrue: Shape.TextBox(
                                Id: "prefix", Name: "Prefix",
                                Title: T("Prefix"),
                                Description: T("Text to put before the number, such as currency symbol.")
                                ),
                            _ValueFalse: Shape.TextBox(
                                Id: "suffix", Name: "Suffix",
                                Title: T("Suffix"),
                                Description: T("Text to put after the number, such as currency symbol.")
                                )
                        ));

                    return f;
                };

            context.Form(FormName, form);

        }

        public static string FormatNumber(decimal number, dynamic state, string culture) {
            var cultureInfo = CultureInfo.CreateSpecificCulture(culture);

            string prefix = state.Prefix;
            string result = number.ToString(cultureInfo);
            
            if(!String.IsNullOrEmpty(prefix)) {
                result = prefix + result;
            }

            string suffix = state.Suffix;
            if (!String.IsNullOrEmpty(suffix)) {
                result = result + suffix;
            }

            return result;
        }
    }
}