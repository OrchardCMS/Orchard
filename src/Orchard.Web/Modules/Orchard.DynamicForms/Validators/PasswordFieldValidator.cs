using System;
using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class PasswordFieldValidator : ElementValidator<PasswordField> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public PasswordFieldValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(PasswordField element) {
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

            if (!String.IsNullOrWhiteSpace(settings.RegularExpression)) {
                yield return _validationRuleFactory.Create<RegularExpression>(r => {
                    r.Pattern = settings.RegularExpression;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }

            if (!String.IsNullOrWhiteSpace(settings.CompareWith)) {
                yield return _validationRuleFactory.Create<Compare>(r => {
                    r.TargetName = settings.CompareWith;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}