using System;
using System.Globalization;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;

namespace Orchard.DynamicForms.ValidationRules {
    public class Range : ValidationRule {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public int? Scale { get; set; }
        public CultureInfo Culture { get; set; }
        
        public override void Validate(ValidateInputContext context) {
            Decimal value;

            if (!Decimal.TryParse(context.AttemptedValue, NumberStyles.Any, Culture, out value)
                || (Min != null && value < Min) || (Max != null && value > Max)
                || (Scale != null && Math.Round(value, Scale.Value) != value)) {
                var message = GetValidationMessage(context);
                context.ModelState.AddModelError(context.FieldName, message.Text);
            }
        }

        public override void RegisterClientAttributes(RegisterClientValidationAttributesContext context) {
            if (Min != null || Max != null) {
                context.ClientAttributes["data-val-range"] = GetValidationMessage(context).Text;
                context.ClientAttributes["data-val-range-min"] = (Min ?? Decimal.MinValue).ToString(CultureInfo.InvariantCulture);
                context.ClientAttributes["data-val-range-max"] = (Max ?? Decimal.MaxValue).ToString(CultureInfo.InvariantCulture);
                context.ClientAttributes["data-val-range-scale"] = (Scale != null && Scale > 0 ? Scale : 0).ToString();
            }
        }

        private LocalizedString GetValidationMessage(ValidationContext context) {
            if (!String.IsNullOrWhiteSpace(ErrorMessage))
                return T(Tokenize(String.Format(ErrorMessage, context.FieldName, Min, Max), context));

            LocalizedString message;
            if (Scale != null && Scale > 0) {

                if (Min != null && Max != null)
                    message = T("{0} must be a number with up to {1} decimals and between {2} and {3}.", context.FieldName, Scale, Min, Max);
                else if (Min != null)
                    message = T("{0} must be a number with up to {1} decimals and greater than or equal to {2}.", context.FieldName, Scale, Min);
                else if (Max != null)
                    message = T("{0} must be a number with up to {1} decimals and less than or equal to {2}.", context.FieldName, Scale, Max);
                else
                    message = T("{0} must be a number with up to {1} decimals.", context.FieldName, Scale);
            }
            else {

                if (Min != null && Max != null)
                    message = T("{0} must be an integer between {1} and {2}.", context.FieldName, Min, Max);
                else if (Min != null)
                    message = T("{0} must be an integer greater than or equal to {1}.", context.FieldName, Min);
                else if (Max != null)
                    message = T("{0} must be an integer less than or equal to {1}.", context.FieldName, Max);
                else
                    message = T("{0} must be an integer.", context.FieldName);
            }

            return message;
        }
    }
}
