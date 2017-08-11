using System;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.FilterEditors.Forms {
    public class DateTimeFilterFormValidation : FormHandler {
        public Localizer T { get; set; }

        private static readonly Regex _dateRegEx = new Regex(@"(\d{1,4}(\-\d{1,2}(\-\d{1,2}\s*(\d{1,2}(:\d{1,2}(:\d{1,2})?)?)?)?)?)|(\{.*\})"); 

        public override void Validating(ValidatingContext context) {
            if (context.FormName == DateTimeFilterForm.FormName) {

                var op = (DateTimeOperator)Enum.Parse(typeof(DateTimeOperator), Convert.ToString(context.ValueProvider.GetValue("Operator").AttemptedValue));

                if (op == DateTimeOperator.IsNull || op == DateTimeOperator.IsNotNull) {
                    // no further validation needed
                }
                else {
                    var isRange = new[] {DateTimeOperator.Between, DateTimeOperator.NotBetween}.Contains(op);
                    var min = context.ValueProvider.GetValue("Min");
                    var max = context.ValueProvider.GetValue("Max");
                    var value = context.ValueProvider.GetValue("Value");
                    var valueType = context.ValueProvider.GetValue("ValueType");

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

                    // validating data type
                    if (valueType.AttemptedValue == "0") {
                        // A date

                        if(isRange) {
                            if(!_dateRegEx.IsMatch(min.AttemptedValue) && !IsToken(min.AttemptedValue)) {
                                context.ModelState.AddModelError("Min", T("The field {0} should contain a valid date (YYYY-MM-DD hh:mm:ss)", T("Min").Text).Text);
                            }

                            if (!_dateRegEx.IsMatch(max.AttemptedValue) && !IsToken(max.AttemptedValue)) {
                                context.ModelState.AddModelError("Max", T("The field {0} should contain a valid date (YYYY-MM-DD hh:mm:ss)", T("Max").Text).Text);
                            }
                        }
                        else {
                            if (!_dateRegEx.IsMatch(value.AttemptedValue) && !IsToken(value.AttemptedValue)) {
                                context.ModelState.AddModelError("Value", T("The field {0} should contain a valid date (YYYY-MM-DD hh:mm:ss)", T("Value").Text).Text);
                            }
                        }

                    }
                    else {
                        // An offset
                        int number;
                        if (isRange) {
                            if (!Int32.TryParse(min.AttemptedValue, out number) && !IsToken(min.AttemptedValue)) {
                                context.ModelState.AddModelError("Min", T("The field {0} must be a valid number.", T("Min").Text).Text);
                            }

                            if (!Int32.TryParse(max.AttemptedValue, out number) && !IsToken(max.AttemptedValue)) {
                                context.ModelState.AddModelError("Max", T("The field {0} must be a valid number.", T("Max").Text).Text);
                            }
                        }
                        else {
                            if (!Int32.TryParse(value.AttemptedValue, out number) && !IsToken(value.AttemptedValue)) {
                                context.ModelState.AddModelError("Value", T("The field {0} must be a valid number.", T("Value").Text).Text);
                            }

                        }
                    }
                }
            }
        }

        private bool IsToken(string value) {
            return value.StartsWith("{") && value.EndsWith("}");
        }

    }
}
