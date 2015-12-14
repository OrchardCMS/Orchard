using System;
using System.Linq;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.FilterEditors.Forms {
    public class NumericFieldTypeEditorValidation : FormHandler {
        public Localizer T { get; set; }

        public override void Validating(ValidatingContext context) {
            if (context.FormName == NumericFilterForm.FormName) {

                var isRange = new[] { "Between", "NotBetween" }.Contains(context.ValueProvider.GetValue("Operator").AttemptedValue);
                var min = context.ValueProvider.GetValue("Min");
                var max = context.ValueProvider.GetValue("Max");
                var value = context.ValueProvider.GetValue("Value");

                // validating mandatory values
                if (isRange) {
                    if (min == null || String.IsNullOrWhiteSpace(min.AttemptedValue)) {
                        context.ModelState.AddModelError("Min", T("The field {0} is required.", T("Min").Text).Text);
                    }

                    if (max == null || String.IsNullOrWhiteSpace(max.AttemptedValue)) {
                        context.ModelState.AddModelError("Max", T("The field {0} is required.", T("Max").Text).Text);
                    }
                }
                else {
                    if (min == null || String.IsNullOrWhiteSpace(value.AttemptedValue)) {
                        context.ModelState.AddModelError("Value", T("The field {0} is required.", T("Value").Text).Text);
                    }
                }

                if (!context.ModelState.IsValid) {
                    return;
                }

                decimal output;

                if (isRange) {
                    if (!Decimal.TryParse(min.AttemptedValue, out output) && !IsToken(min.AttemptedValue)) {
                        context.ModelState.AddModelError("Min", T("The field {0} should contain a valid number", T("Min").Text).Text);
                    }

                    if (!Decimal.TryParse(max.AttemptedValue, out output) && !IsToken(max.AttemptedValue)) {
                        context.ModelState.AddModelError("Max", T("The field {0} should contain a valid number", T("Max").Text).Text);
                    }
                }
                else {
                    if (!Decimal.TryParse(value.AttemptedValue, out output) && !IsToken(value.AttemptedValue)) {
                        context.ModelState.AddModelError("Value", T("The field {0} should contain a valid number", T("Value").Text).Text);
                    }
                }
            }
        }

        private bool IsToken(string value) {
            return value.StartsWith("{") && value.EndsWith("}");
        }
    }
}