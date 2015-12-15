using System;
using System.Text.RegularExpressions;
using Orchard.DynamicForms.Helpers;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;

namespace Orchard.DynamicForms.ValidationRules {
    public class EmailAddress : ValidationRule {
        public EmailAddress() {
            RegexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase;
            Pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$";
        }

        public string Pattern { get; set; }
        public RegexOptions RegexOptions { get; set; }

        public override void Validate(ValidateInputContext context) {
            if (!Regex.IsMatch(context.AttemptedValue, Pattern, RegexOptions)) {
                var message = GetValidationMessage(context);
                context.ModelState.AddModelError(context.FieldName, message.Text);
            }
        }

        public override void RegisterClientAttributes(RegisterClientValidationAttributesContext context) {
            context.ClientAttributes["data-val-regex"] = GetValidationMessage(context).Text;
            context.ClientAttributes["data-val-regex-pattern"] = Pattern;
        }

        private LocalizedString GetValidationMessage(ValidationContext context) {
            return String.IsNullOrWhiteSpace(ErrorMessage)
                ? T("{0} is not a valid email address.", context.FieldName)
                : T(ErrorMessage);
        }
    }
}