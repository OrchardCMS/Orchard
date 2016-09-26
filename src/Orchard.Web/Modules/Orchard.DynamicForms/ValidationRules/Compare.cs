using System;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;

namespace Orchard.DynamicForms.ValidationRules {
    public class Compare : ValidationRule {
        public string TargetName { get; set; }

        public override void Validate(ValidateInputContext context) {
            var targetValue = context.Values[TargetName];
            if (!String.Equals(context.AttemptedValue, targetValue)) {
                var message = GetValidationMessage(context);
                context.ModelState.AddModelError(context.FieldName, message.Text);
            }
        }

        public override void RegisterClientAttributes(RegisterClientValidationAttributesContext context) {
            context.ClientAttributes["data-val-equalto"] = GetValidationMessage(context).Text;
            context.ClientAttributes["data-val-equalto-other"] = "*." + TargetName;
        }

        private LocalizedString GetValidationMessage(ValidationContext context) {
            return T(Tokenize(ErrorMessage.WithDefault(String.Format("{0} must match the value of {1}.", context.FieldName, TargetName)), context));
        }
    }
}