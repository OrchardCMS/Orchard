using System;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;

namespace Orchard.DynamicForms.ValidationRules {
    public class Mandatory : ValidationRule {
        public override void Validate(ValidateInputContext context) {
            if (String.IsNullOrWhiteSpace(context.AttemptedValue)) {
                var message = GetValidationMessage(context);
                context.ModelState.AddModelError(context.FieldName, message.Text);
            }
        }

        public override void RegisterClientAttributes(RegisterClientValidationAttributesContext context) {
            context.ClientAttributes["data-val-mandatory"] = GetValidationMessage(context).Text;
        }

        private LocalizedString GetValidationMessage(ValidationContext context) {
            return String.IsNullOrWhiteSpace(ErrorMessage)
                ? T("{0} is a required field.", context.FieldName)
                : T(ErrorMessage);
        }
    }
}