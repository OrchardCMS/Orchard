using System;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Validators {
    public class CheckBoxValidator : ElementValidator<CheckBox> {
        protected override void OnValidate(CheckBox element, ValidateInputContext context) {
            var settings = element.ValidationSettings;

            if (settings.IsMandatory != true)
                return;

            if (String.IsNullOrWhiteSpace(context.AttemptedValue)) {
                var message = GetValidationMessage(settings);
                context.ModelState.AddModelError(context.FieldName, T(message, context.FieldName).Text);
            }
        }

        protected override void OnRegisterClientValidation(CheckBox element, RegisterClientValidationAttributesEventContext context) {
            var settings = element.ValidationSettings;

            if (settings.IsMandatory != true)
                return;

            var message = GetValidationMessage(settings);

            context.ClientAttributes["data-val"] = "true";
            context.ClientAttributes["data-val-mandatory"] = T(message, context.Element.Name).Text;
        }

        private string GetValidationMessage(CheckBoxValidationSettings settings) {
            var message = String.IsNullOrWhiteSpace(settings.CustomValidationMessage)
                    ? "{0} is a mandatory field."
                    : settings.CustomValidationMessage;

            return message;
        }
    }
}