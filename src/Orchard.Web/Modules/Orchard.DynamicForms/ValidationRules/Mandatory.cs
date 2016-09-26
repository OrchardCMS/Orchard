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
            return T(Tokenize(ErrorMessage.WithDefault(String.Format("{0} is a mandatory field.", context.FieldName)), context));
        }
    }
}