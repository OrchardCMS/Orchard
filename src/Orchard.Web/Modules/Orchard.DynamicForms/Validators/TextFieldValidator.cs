using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class TextFieldValidator : ElementValidator<TextField> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public TextFieldValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(TextField element) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired == true)
                yield return _validationRuleFactory.Create<Required>(settings.CustomValidationMessage);

            if (settings.MinimumLength != null || settings.MaximumLength != null) {
                yield return _validationRuleFactory.Create<StringLength>(r => {
                    r.Minimum = settings.MinimumLength;
                    r.Maximum = settings.MaximumLength;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
            if (!string.IsNullOrWhiteSpace(settings.ValidationExpression)) {
                yield return _validationRuleFactory.Create<RegularExpression>(r => {
                    r.Pattern = settings.ValidationExpression;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}