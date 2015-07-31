using System;
using System.Globalization;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;

namespace Orchard.DynamicForms.ValidationRules {
    public class StringLength : ValidationRule {
        public int? Minimum { get; set; }
        public int? Maximum { get; set; }
        
        public override void Validate(ValidateInputContext context) {
            if (Minimum != null) {
                if (context.AttemptedValue == null || context.AttemptedValue.Length < Minimum) {
                    var message = GetValidationMessage(context);
                    context.ModelState.AddModelError(context.FieldName, message.Text);
                }
            }

            if (Maximum != null) {
                if (context.AttemptedValue == null || context.AttemptedValue.Length > Maximum) {
                    var message = GetValidationMessage(context);
                    context.ModelState.AddModelError(context.FieldName, message.Text);
                }
            }
        }

        public override void RegisterClientAttributes(RegisterClientValidationAttributesContext context) {
            if (Minimum != null || Maximum != null) {
                context.ClientAttributes["data-val-length"] = GetValidationMessage(context).Text;

                if (Minimum != null) {
                    context.ClientAttributes["data-val-length-min"] = Minimum.Value.ToString(CultureInfo.InvariantCulture);
                }

                if (Maximum != null) {
                    context.ClientAttributes["data-val-length-max"] = Maximum.Value.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private LocalizedString GetValidationMessage(ValidationContext context) {
            if (!String.IsNullOrWhiteSpace(ErrorMessage))
                return T(Tokenize(String.Format(ErrorMessage, context.FieldName, Minimum, Maximum), context));

            if(Minimum != null && Maximum != null)
                return T("{0} must be between {1} and {2} characters long.", context.FieldName, Minimum, Maximum);
            else if (Minimum != null)
                return T("{0} must be at least {1} characters long.", context.FieldName, Minimum);
            
            return T("{0} must be at most {1} characters long.", context.FieldName, Maximum);
        }
    }
}
