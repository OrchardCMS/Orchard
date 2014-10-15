using System;
using System.Globalization;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.DynamicForms.Validators.Settings;

namespace Orchard.DynamicForms.Validators {
    public class TextFieldValidator : ElementValidator<TextField> {
        protected override void OnValidate(TextField element, ValidateInputContext context) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired != true)
                return;

            if (String.IsNullOrWhiteSpace(context.AttemptedValue)) {    
                var message = GetValidationMessage(settings);
                context.ModelState.AddModelError(context.FieldName, T(message, context.FieldName).Text);
            }
        }

        protected override void OnRegisterClientValidation(TextField element, RegisterClientValidationAttributesEventContext context) {
            var settings = element.ValidationSettings;
            
            if (settings.IsRequired != true && settings.MinimumLength == null && settings.MaximumLength == null)
                return;

            var message = GetValidationMessage(settings);

            context.ClientAttributes["data-val"] = "true";

            if(settings.IsRequired == true)
                context.ClientAttributes["data-val-required"] = T(message, context.Element.Name).Text;

            if (settings.MaximumLength != null || settings.MinimumLength != null) {
                if (settings.MaximumLength != null) {
                    context.ClientAttributes["data-val-length-max"] = settings.MaximumLength.Value.ToString(CultureInfo.InvariantCulture);
                }

                if (settings.MinimumLength != null) {
                    context.ClientAttributes["data-val-length-min"] = settings.MinimumLength.Value.ToString(CultureInfo.InvariantCulture);
                }

                context.ClientAttributes["data-val-length"] = T(message, element.Name, settings.MaximumLength, settings.MinimumLength).Text;
            }
        }

        private string GetValidationMessage(TextFieldValidationSettings validationSettings) {
            var message = String.IsNullOrWhiteSpace(validationSettings.CustomValidationMessage)
                    ? "{0} is a required field."
                    : validationSettings.CustomValidationMessage;

            return message;
        }
    }
}